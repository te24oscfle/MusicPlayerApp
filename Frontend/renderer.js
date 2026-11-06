async function main() {
    const albums = await window.api.getAlbums()
    albums.forEach(album => {
        print(album)
    });
}

main()