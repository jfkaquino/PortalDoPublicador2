// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
self.addEventListener('fetch', () => { });

// Importa os módulos separados
importScripts(
    './js/sync-db.js',
    './js/sync-queue.js'
);

self.addEventListener('sync', event => {
    if (event.tag === 'sync-queue') {
        event.waitUntil(syncQueue(event.db));
    }
});

async function processarSincronizacaoCompleta() {
    const db = await abrirBanco('MeuAppOfflineDb', 2);
    await syncQueue(db);
}