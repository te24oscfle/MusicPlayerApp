const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('api', {
    importFromPath: (path) => ipcRenderer.invoke("import-from-path", path),
    getAlbums: () => ipcRenderer.invoke("get-albums"),
    getAlbum: (albumId) => ipcRenderer.invoke("get-album", albumId),
    getPlaylists: () => ipcRenderer.invoke("get-playlists"),
    deletePlaylists: (playlistIds) => ipcRenderer.invoke("delete-playlists", playlistIds),
    getPlaylist: (playlistId) => ipcRenderer.invoke("get-playlist", playlistId),
    createPlaylist: (requestBody) => ipcRenderer.invoke("create-playlist", requestBody),
    updatePlaylistMetadata: (requestBody) => ipcRenderer.invoke("update-playlist-metadata", requestBody),
    deletePlaylist: (playlistId) => ipcRenderer.invoke("delete-playlist", playlistId),
    addPlaylistTracks: (requestBody) => ipcRenderer.invoke("add-playlist-tracks", requestBody),
    movePlaylistTracks: (requestBody) => ipcRenderer.invoke("move-playlist-tracks", requestBody),
    removePlaylistTracks: (requestBody) => ipcRenderer.invoke("remove-playlist-tracks", requestBody),

    getQueue: () => ipcRenderer.invoke("get-queue"),
    addQueueTracks: (trackIds) => ipcRenderer.invoke("add-queue-tracks", trackIds),
    moveQueueTracks: (requestBody) => ipcRenderer.invoke("move-queue-tracks", requestBody),
    removeQueueTracks: (positions) => ipcRenderer.invoke("remove-queue-tracks", positions),
    clearQueue: () => ipcRenderer.invoke("clear-queue"),
    setLoopState: (loopState) => ipcRenderer.invoke("set-loop-state", loopState),
    setShuffleState: (shuffleState) => ipcRenderer.invoke("set-shuffle-state", shuffleState),
    playAlbum: (albumId) => ipcRenderer.invoke("play-album", albumId),
    playPlaylist: (playlistId) => ipcRenderer.invoke("play-playlist", playlistId),

    getPlaybackState: () => ipcRenderer.invoke("get-playback-state"),
    play: () => ipcRenderer.invoke("play"),
    pause: () => ipcRenderer.invoke("pause"),
    next: () => ipcRenderer.invoke("next"),
    previous: () => ipcRenderer.invoke("previous"),
    setVolume: (volume) => ipcRenderer.invoke("set-volume", volume),
    seek: (positionInSeconds) => ipcRenderer.invoke("seek", positionInSeconds)
})