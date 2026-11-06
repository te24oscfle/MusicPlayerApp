using MusicLibrary;

namespace API;

public static class LibraryEndpoints
{
    public static void MapLibraryEndpoints(this WebApplication app)
    {
        app.MapGet("/api/library/albums", () =>
        {
            HashSet<Album> albums = Library.GetAlbums();
            return Results.Ok(albums);
        });
    }
}