async function testLibraryEndpoints() {
    // ImportFromPath
    const importResponse = await window.api.importFromPath("E:/Music Library/Hurula/Klass")
    console.log(`ImportFromPath Response`)
    console.log(importResponse)

    // GetAlbums
    const getAlbumsResponse = await window.api.getAlbums()
    console.log("GetAlbums Response:")
    console.log(getAlbumsResponse)

    // GetAlbum
    const getAlbumResponse = await window.api.getAlbum(1)
    console.log("GetAlbum Response:")
    console.log(getAlbumResponse)

    // GetPlaylists
    const getPlaylistsResponse = await window.api.getPlaylists()
    console.log(`GetPlaylists Response:`)
    console.log(getPlaylistsResponse)

    // DeletePlaylists
    // const deletePlaylistsRespone = await window.api.deletePlaylists([2, 3, 4, 5, 6, 7])
    // console.log(`DeletePlaylistsRespone Response:`)
    // console.log(deletePlaylistsRespone)

    // CreatePlaylist
    const playlist = await window.api.createPlaylist({
        playlistTitle: "My Playlist",
        playlistType: "Manual"
    })
    console.log(`CreatePlaylist Response:`)
    console.log(playlist)

    console.log(playlist.playlistId)

    // UpdatePlaylistMetadata
    const updatePlaylistResponse = await window.api.updatePlaylistMetadata({
        playlistId: playlist.playlistId,
        title: "My Favourite Playlist",
        description: "This is my favourite playlist with my favourite songs in it.",
        imageFilePath: "C:/Users/Oscar/Pictures/Blue Emojis/01"
    })
    console.log(`updatePlaylist Response:`)
    console.log(updatePlaylistResponse)

    // AddPlaylistTracks
    const addPlaylistTracksResponse = await window.api.addPlaylistTracks({
        playlistId: playlist.playlistId,
        trackIds: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
    })
    console.log(`AddPlaylistTracks Response:`)
    console.log(addPlaylistTracksResponse)

    // MovePlaylistTracks
    const movePlaylistTracksResponse = await window.api.movePlaylistTracks({
        playlistId: playlist.playlistId,
        positions: [7, 8, 9],
        targetIndex: 4
    })
    console.log(`MovePlaylistTracks Response:`)
    console.log(movePlaylistTracksResponse)

    // RemovePlaylistTracks
    const removePlaylistTracksResponse = await window.api.removePlaylistTracks({
        playlistId: playlist.playlistId,
        positions: [7, 8, 9]
    })
    console.log(`RemovePlaylistTracks Response:`)
    console.log(removePlaylistTracksResponse)

    // GetPlaylist
    const getPlaylistResponse = await window.api.getPlaylist(playlist.playlistId)
    console.log(`GetPlaylist Response:`)
    console.log(getPlaylistResponse)

    // DeletePlaylist
    const deletePlaylistResponse = await window.api.deletePlaylist(playlist.playlistId)
    console.log(`DeletePlaylist Response:`)
    console.log(deletePlaylistResponse)
}

async function testQueueEndpoints() {
    // Add track to queue
    const addTrackResponse = await window.api.addQueueTracks([1, 2, 3, 4, 5, 6, 7, 8, 9, 10])
    console.log("AddQueueTracks Response:")
    console.log(addTrackResponse)
    
    // Move tracks
    const moveTracksResponse = await window.api.moveQueueTracks({
        positions: [7, 8, 9],
        targetIndex: 4
    })
    console.log("MoveQueueTracks Response:")
    console.log(moveTracksResponse)

    // Remove Tracks
    const removeTracksResponse = await window.api.removeQueueTracks([7, 8, 9])
    console.log("RemoveQueueTracks Response:")
    console.log(removeTracksResponse)

    // Get Queue
    const getQueueResponse = await window.api.getQueue()
    console.log("GetQueue Response:")
    console.log(getQueueResponse)

    // Set Loop State
    const setLoopResponse = await window.api.setLoopState("Disabled")
    console.log("SetLoopState Response:")
    console.log(setLoopResponse)

    // Set Shuffle State
    const setShuffleResponse = await window.api.setShuffleState("Enabled")
    console.log("SetShuffleState Response:")
    console.log(setShuffleResponse)

    // Clear Queue
    const clearQueueResponse = await window.api.clearQueue()
    console.log("ClearQueue Response:")
    console.log(clearQueueResponse)

    console.log("Queue:")
    console.log(await window.api.getQueue())

    // Play Album
    const playAlbumResponse = await window.api.playAlbum(1)
    console.log("PlayAlbum Response:")
    console.log(playAlbumResponse)

    // Play Playlist
    setTimeout(async () => {
        const playPlaylistResponse = await window.api.playPlaylist(1)
        console.log("PlayPlaylist Response:")
        console.log(playPlaylistResponse)
    }, 5 * 1000)
}

async function testPlaybackEndpoints() {
    const INTERVAL_DURATION = 3 * 1000
    
    await window.api.playAlbum(1)

    const playbackState = await window.api.getPlaybackState()
    console.log("PlaybackState:")
    console.log(playbackState)

    await setTimeout(async () => {
        await window.api.pause()
        const playbackState = await window.api.getPlaybackState()
        console.log("PlaybackState:")
        console.log(playbackState)
    }, INTERVAL_DURATION)

    await setTimeout(async () => {
        await window.api.play()
        const playbackState = await window.api.getPlaybackState()
        console.log("PlaybackState:")
        console.log(playbackState)
    }, INTERVAL_DURATION*2)

    await setTimeout(async () => {
        await window.api.next()
        const playbackState = await window.api.getPlaybackState()
        console.log("PlaybackState:")
        console.log(playbackState)
    }, INTERVAL_DURATION*3)

    await setTimeout(async () => {
        await window.api.previous()
        const playbackState = await window.api.getPlaybackState()
        console.log("PlaybackState:")
        console.log(playbackState)
    }, INTERVAL_DURATION*4)

    await setTimeout(async () => {
        await window.api.setVolume(25)
        const playbackState = await window.api.getPlaybackState()
        console.log("PlaybackState:")
        console.log(playbackState)
    }, INTERVAL_DURATION*5)

    await setTimeout(async () => {
        await window.api.seek(180)
        const playbackState = await window.api.getPlaybackState()
        console.log("PlaybackState:")
        console.log(playbackState)
    }, INTERVAL_DURATION*6)
}

// testLibraryEndpoints()
// testQueueEndpoints()
// testPlaybackEndpoints()