window.GrooveDrive = window.GrooveDrive || {};

GrooveDrive._callbacker = function (callbackObjectInstance, callbackMethod, callbackId, cmd, args) {
    var parts = cmd.split('.');
    var targetFunc = window;
    var parentObject = window;
    for (var i = 0; i < parts.length; i++) {
        if (i == 0 && part == 'window') continue;
        var part = parts[i];
        parentObject = targetFunc;
        targetFunc = targetFunc[part];
    }
    args = JSON.parse(args);
    args.push(function (e, d) {
        var args = [];
        for (var i in arguments) args.push(JSON.stringify(arguments[i]));
        callbackObjectInstance.invokeMethodAsync(callbackMethod, callbackId, args);
    });
    targetFunc.apply(parentObject, args);
};

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

GrooveDrive.indexedDB.init = (instance, callbackMethod, dbName, tableId, keyPath, version) => {
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
                instance.invokeMethodAsync(callbackMethod, songs);
            }
        };
    };
};

GrooveDrive.indexedDB.add = (tableId, data) => {
    if (data && data.length > 0) {
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
    }
};

GrooveDrive.indexedDB.delete = (instance, callbackMethod, databaseName) => {
    let req = indexedDB.deleteDatabase(databaseName);

    req.onsuccess = function (ev) {
        alert("Deleted database successfully");
        instance.invokeMethodAsync(callbackMethod, true);
    };

    req.onerror = function (ev) {
        alert("Couldn't delete database");
    };

    req.onblocked = function (ev) {
        GrooveDrive.indexedDB.db.close();
        GrooveDrive.indexedDB.delete(instance, callbackMethod, databaseName);
    };
};
