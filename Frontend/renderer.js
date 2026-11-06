async function getAlbums() {
    return await window.api.getAlbums()
}

const albums = getAlbums()
console.log(albums)