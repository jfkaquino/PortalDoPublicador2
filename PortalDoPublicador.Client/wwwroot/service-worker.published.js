// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
self.importScripts('./js/indexedDbQueue.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [ /\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/, /\.webmanifest$/ ];
const offlineAssetsExclude = [ /^service-worker\.js$/ ];

// Replace with your base path if you are hosting on a subfolder. Ensure there is a trailing '/'.
const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall(event) {
    console.info('Service worker: Install');

    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
}

async function onActivate(event) {
    console.info('Service worker: Activate');

    // Delete unused caches
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
}

async function onFetch(event) {
    let cachedResponse = null;
    if (event.request.method === 'GET') {
        // For all navigation requests, try to serve index.html from cache,
        // unless that request is for an offline resource.
        // If you need some URLs to be server-rendered, edit the following check to exclude those URLs
        const shouldServeIndexHtml = event.request.mode === 'navigate'
            && !manifestUrlList.some(url => url === event.request.url);

        const request = shouldServeIndexHtml ? 'index.html' : event.request;
        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(request);
    }

    return cachedResponse || fetch(event.request);
}

// Background Sync Event
self.addEventListener('sync', (event) => {
    if (event.tag === 'sync-changes') {
        event.waitUntil(processSyncQueue());
    }
});

// Fallback message event
self.addEventListener('message', (event) => {
    if (event.data && event.data.type === 'SYNC_NOW') {
        processSyncQueue();
    }
});

async function processSyncQueue() {
    if (!self.syncQueue) return;
    
    try {
        const changes = await self.syncQueue.getAllChanges();
        if (changes && changes.length > 0) {
            const apiBaseUrl = 'http://portaldopublicador-api'; // Or relative depending on hosting
            // Note: Using the Aspire service discovery name as used in Program.cs
            const response = await fetch('http://portaldopublicador-api/api/sync', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(changes)
            });
            
            if (response.ok) {
                await self.syncQueue.clearQueue();
                console.log('Sync completed successfully.');
            } else {
                if (response.status === 409) {
                    const erroConflito = await response.json();
                    console.warn("Conflito detectado no servidor", erroConflito);
                    
                    const clients = await self.clients.matchAll();
                    clients.forEach(client => {
                        client.postMessage({
                            type: 'CONFLITO_RESOLVIDO_PELO_SERVIDOR',
                            detalhes: erroConflito
                        });
                    });
                    
                    // Limpamos a fila para não entrar em loop infinito travando os próximos syncs
                    await self.syncQueue.clearQueue();
                }
                console.error('Sync API returned an error:', response.status);
            }
        }
    } catch (err) {
        console.error('Error processing sync queue', err);
    }
}
