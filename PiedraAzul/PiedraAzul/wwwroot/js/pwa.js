// pwa.js — corre en la app normal (Blazor)
// Registra el Service Worker y expone helpers de IndexedDB para C#

// ── Registro del Service Worker ───────────────────────────────────────────────
if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
        navigator.serviceWorker.register('/sw.js')
            .then(reg => console.log('[PWA] Service Worker registrado:', reg.scope))
            .catch(err => console.warn('[PWA] Error registrando SW:', err));
    });
}

// ── IndexedDB — helpers compartidos ─────────────────────────────────────────
function openPiedraAzulDB() {
    return new Promise((resolve, reject) => {
        const req = indexedDB.open('PiedraAzulDB', 1);
        req.onupgradeneeded = (e) => {
            const db = e.target.result;
            if (!db.objectStoreNames.contains('cache')) {
                db.createObjectStore('cache', { keyPath: 'key' });
            }
        };
        req.onsuccess = () => resolve(req.result);
        req.onerror   = () => reject(req.error);
    });
}

// Llamado desde C# via JS interop al hacer login o crear cita
window.saveAppointmentsToIndexedDB = async (appointments) => {
    try {
        const db    = await openPiedraAzulDB();
        const tx    = db.transaction('cache', 'readwrite');
        const store = tx.objectStore('cache');
        store.put({ key: 'upcoming_appointments', data: appointments, timestamp: Date.now() });
        return true;
    } catch (e) {
        console.warn('[PWA] Error guardando citas:', e);
        return false;
    }
};

// Llamado desde C# para leer citas offline
window.getAppointmentsFromIndexedDB = async () => {
    try {
        const db    = await openPiedraAzulDB();
        return new Promise((resolve) => {
            const tx    = db.transaction('cache', 'readonly');
            const store = tx.objectStore('cache');
            const req   = store.get('upcoming_appointments');
            req.onsuccess = () => resolve(req.result?.data ?? null);
            req.onerror   = () => resolve(null);
        });
    } catch (e) {
        return null;
    }
};

// Verificar si hay conexión
window.isOnline = () => navigator.onLine;
