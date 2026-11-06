const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('api', {

    getAlbums: () => ipcRenderer.invoke("get-albums")

})