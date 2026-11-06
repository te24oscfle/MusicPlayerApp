async function main() {
    const albums = await window.api.getAlbums()
    albums.forEach(album => {
        console.log(album.albumCover)
    });
}

main()
