// offline.js — corre SOLO en offline.html
// Lee IndexedDB y puebla la lista de citas

document.addEventListener('DOMContentLoaded', async () => {
    const list       = document.getElementById('appointments-list');
    const noAppts    = document.getElementById('no-appointments');
    const retryBtn   = document.getElementById('retry-btn');

    // ── 1. Cargar citas desde IndexedDB ──────────────────────────────────────
    let appointments = [];
    try {
        appointments = await getAppointments() ?? [];
    } catch (e) {
        console.warn('[Offline] No se pudo leer IndexedDB:', e);
    }

    if (appointments.length > 0) {
        renderAppointments(appointments, list);
    } else {
        list.classList.add('hidden');
        noAppts.classList.remove('hidden');
    }

    // ── 2. Botón Reintentar ───────────────────────────────────────────────────
    retryBtn.addEventListener('click', () => {
        if (navigator.onLine) {
            window.location.href = '/';
        } else {
            retryBtn.textContent = 'Aún sin conexión…';
            retryBtn.disabled = true;
            setTimeout(() => {
                retryBtn.textContent = 'Reintentar conexión';
                retryBtn.disabled = false;
            }, 3000);
        }
    });

    // ── 3. Volver automáticamente cuando hay internet ─────────────────────────
    window.addEventListener('online', () => {
        window.location.href = '/';
    });
});

// ── Render ──────────────────────────────────────────────────────────────────
function renderAppointments(appointments, container) {
    const MONTHS_ES = [
        'enero','febrero','marzo','abril','mayo','junio',
        'julio','agosto','septiembre','octubre','noviembre','diciembre'
    ];

    appointments.forEach((cita) => {
        const d    = new Date(cita.date ?? cita.Date);
        const day  = d.getDate();
        const mon  = MONTHS_ES[d.getMonth()];
        const year = d.getFullYear();
        const time = cita.time ?? cita.Time ?? '';
        const doctor    = cita.doctorName   ?? cita.DoctorName   ?? 'Doctor';
        const specialty = cita.specialty    ?? cita.Specialty    ?? '';
        const status    = cita.status       ?? cita.Status       ?? 'Pendiente';

        const statusColor = status.toLowerCase() === 'confirmed'
            ? '#10b981' : '#f59e0b';

        const card = document.createElement('div');
        card.className = 'rounded-xl border border-gray-100 bg-white p-5 shadow-sm';
        card.innerHTML = `
            <div class="mb-3 flex items-center justify-between">
                <span class="inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-[11px] font-semibold text-white"
                      style="background:${statusColor}">
                    ${status}
                </span>
                <span class="text-xs text-gray-400">${day} de ${mon} ${year}</span>
            </div>
            <p class="text-[15px] font-bold text-gray-900">${doctor}</p>
            <p class="mt-0.5 text-xs text-gray-500">${specialty}</p>
            <div class="mt-3 flex items-center gap-1.5 text-sm font-medium" style="color:#257D8D">
                <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                    <path stroke-linecap="round" stroke-linejoin="round"
                          d="M12 6v6l4 2m6-2a10 10 0 11-20 0 10 10 0 0120 0z"/>
                </svg>
                ${time}
            </div>
        `;
        container.appendChild(card);
    });
}

// ── IndexedDB helpers ────────────────────────────────────────────────────────
function openDB() {
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

async function getAppointments() {
    const db = await openDB();
    return new Promise((resolve, reject) => {
        const tx    = db.transaction('cache', 'readonly');
        const store = tx.objectStore('cache');
        const req   = store.get('upcoming_appointments');
        req.onsuccess = () => resolve(req.result?.data ?? null);
        req.onerror   = () => reject(req.error);
    });
}
