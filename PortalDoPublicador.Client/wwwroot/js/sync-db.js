// wwwroot/js/sync-db.js

function abrirBanco(nome, versao) {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(nome, versao);
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

function dbLerTodos(db, storeName) {
    return new Promise((resolve) => {
        const req = db.transaction(storeName, 'readonly').objectStore(storeName).getAll();
        req.onsuccess = () => resolve(req.result);
    });
}

function dbLerItem(db, storeName, id) {
    return new Promise((resolve) => {
        const req = db.transaction(storeName, 'readonly').objectStore(storeName).get(id);
        req.onsuccess = () => resolve(req.result);
    });
}

function dbSalvarItem(db, storeName, item) {
    return new Promise((resolve) => {
        const req = db.transaction(storeName, 'readwrite').objectStore(storeName).put(item);
        req.onsuccess = () => resolve();
    });
}