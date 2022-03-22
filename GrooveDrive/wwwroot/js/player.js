window.GrooveDrive = window.GrooveDrive || {};
GrooveDrive.player = () => document.getElementById('player');
GrooveDrive.streamItem = async function (contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    GrooveDrive.player().src = url;
};
GrooveDrive.play = () => GrooveDrive.player().play();
GrooveDrive.pause = () => GrooveDrive.player().pause();

GrooveDrive.createObject = (object, name) => GrooveDrive[name] = object;
window.GrooveDrive.indexedDB = window.GrooveDrive.indexedDB || {};

GrooveDrive.indexedDB.init = (dbName, tableId, keyPath, version) => {
    window.indexedDB = window.indexedDB || window.mozIndexedDB || window.webkitIndexedDB || window.msIndexedDB;
    window.IDBTransaction = window.IDBTransaction || window.webkitIDBTransaction || window.msIDBTransaction;
    window.IDBKeyRange = window.IDBKeyRange || window.webkitIDBKeyRange || window.msIDBKeyRange

    if (!window.indexedDB) {
        window.alert("Your browser doesn't support a stable version of IndexedDB.")
    }

    var request = indexedDB.open(dbName, version);

    request.onerror = function (event) {
        console.warn(`Error while 'GrooveDrive.indexedDB.create'!`, event);
    };

    request.onupgradeneeded = (event) => {
        GrooveDrive.indexedDB.db = event.target.result;
        GrooveDrive.indexedDB.objectStore = GrooveDrive.indexedDB.db.createObjectStore(tableId, { keyPath: keyPath });
        /*
        indexes.forEach((value, index, array) => {
            GrooveDrive.indexedDB.objectStore.createIndex(value, value, { unique: false });
        });

        data.forEach((value, index, array) => {
            GrooveDrive.indexedDB.objectStore.add(value);
        });*/
    };

    request.onsuccess = (event) => {
        var songs = [];

        GrooveDrive.indexedDB.db = request.result;
        GrooveDrive.indexedDB.objectStore = GrooveDrive.indexedDB.db.transaction([tableId], "readwrite").objectStore(tableId);

        GrooveDrive.indexedDB.objectStore.openCursor().onsuccess = function (event) {
            var cursor = event.target.result;

            if (cursor) {
                songs.push(cursor.value);
                cursor.continue();
            } else {
                console.info(`Loading ${songs.length} songs from IndexedDB...`);
                DotNet.invokeMethodAsync('GrooveDrive', 'ReceiveSongsFromIndexedDB', songs);
            }
        };
    };
};

GrooveDrive.indexedDB.add = (tableId, data) => {
    console.log(`Adding ${data.length} items to table '${tableId}'...`);
    data.forEach((value) => {
        var request = GrooveDrive.indexedDB.db.transaction([tableId], "readwrite")
            .objectStore(tableId)
            .add(value);

        request.onsuccess = function (event) {
            console.log(`'${value.driveId}' has been added to your database.`);
        };

        request.onerror = function (event) {
            console.warn(`Unable to add data\r\n'${value.driveId}' already exists in your database!`);
        }
    });
};
