const { contextBridge, ipcRenderer} = require("electron");

contextBridge.executeInMainWorld("api" , {

    getAlbums: () => ipcRenderer.invoke("get-albums")

})