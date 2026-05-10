// sw.js — Service Worker de Piedra Azul
// Estrategia: cache-first para assets estáticos,
//             network-only para API/GraphQL/SignalR,
//             offline.html para navegación sin conexión.

const CACHE_NAME  = 'piedraazul-v1';
const OFFLINE_URL = '/offline.html';

// Assets que se cachean en install (siempre disponibles offline)
const PRECACHE_URLS = [
    OFFLINE_URL,
    '/app.css',
    '/output.css',
    '/favicon.png',
    '/js/offline.js',
    '/js/pwa.js',
];

// ── INSTALL ──────────────────────────────────────────────────────────────────
self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => {
            return cache.addAll(PRECACHE_URLS);
        })
    );
    self.skipWaiting();
});

// ── ACTIVATE ─────────────────────────────────────────────────────────────────
self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then((cacheNames) =>
            Promise.all(
                cacheNames
                    .filter(name => name !== CACHE_NAME)
                    .map(name => caches.delete(name))
            )
        )
    );
    self.clients.claim();
});

// ── FETCH ─────────────────────────────────────────────────────────────────────
self.addEventListener('fetch', (event) => {
    const { request } = event;
    const url = new URL(request.url);

    // 1. Ignorar: solicitudes no-GET, chrome-extension, etc.
    if (request.method !== 'GET') return;
    if (!url.protocol.startsWith('http'))  return;

    // 2. Ignorar: API, GraphQL, SignalR, Blazor framework
    //    Estas SIEMPRE van al servidor; si falla, falla.
    const skipPaths = ['/api/', '/graphql', '/_blazor', '/_framework', '/hubs/'];
    if (skipPaths.some(p => url.pathname.startsWith(p))) return;

    // 3. Assets estáticos conocidos → cache-first
    const isStaticAsset = (
        url.pathname.endsWith('.css')  ||
        url.pathname.endsWith('.js')   ||
        url.pathname.endsWith('.png')  ||
        url.pathname.endsWith('.jpg')  ||
        url.pathname.endsWith('.svg')  ||
        url.pathname.endsWith('.woff2')||
        url.pathname.endsWith('.ico')
    );

    if (isStaticAsset) {
        event.respondWith(
            caches.match(request).then(cached =>
                cached ?? fetch(request).then(response => {
                    // Cachea asset nuevo para la próxima vez
                    const clone = response.clone();
                    caches.open(CACHE_NAME).then(c => c.put(request, clone));
                    return response;
                })
            )
        );
        return;
    }

    // 4. Navegación (rutas de la app) → network-first, offline.html como fallback
    if (request.mode === 'navigate') {
        event.respondWith(
            fetch(request).catch(() => caches.match(OFFLINE_URL))
        );
        return;
    }

    // 5. Todo lo demás → network-first sin fallback especial
    event.respondWith(fetch(request).catch(() => new Response('', { status: 503 })));
});
