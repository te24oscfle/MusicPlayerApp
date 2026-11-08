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

testLibraryEndpoints()
