window.SincronizacaoOffline = {
    registrarSync: function () {
        if ('serviceWorker' in navigator && 'SyncManager' in window) {
            navigator.serviceWorker.ready.then(reg => {
                reg.sync.register('sync-queue');
            });
        } else {
            console.warn("Background Sync não suportado neste navegador.");
            // Aqui você poderia colocar um fallback, como tentar enviar via fetch na hora.
        }
    }
};