const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('api', {
    importFromPath: (path) => ipcRenderer.invoke("import-from-path", path),
    getAlbums: () => ipcRenderer.invoke("get-albums"),
    getAlbum: (albumId) => ipcRenderer.invoke("get-album", albumId),
    getPlaylists: () => ipcRenderer.invoke("get-playlists"),
    getPlaylist: (playlistId) => ipcRenderer.invoke("get-playlist", playlistId),
    createPlaylist: (requestBody) => ipcRenderer.invoke("create-playlist", requestBody),
    updatePlaylistMetadata: (requestBody) => ipcRenderer.invoke("update-playlist-metadata", requestBody),
    deletePlaylist: (playlistId) => ipcRenderer.invoke("delete-playlist", playlistId),
    addPlaylistTracks: (requestBody) => ipcRenderer.invoke("add-playlist-tracks", requestBody),
    movePlaylistTracks: (requestBody) => ipcRenderer.invoke("move-playlist-tracks", requestBody),
    removePlaylistTracks: (requestBody) => ipcRenderer.invoke("remove-playlist-tracks", requestBody)
})