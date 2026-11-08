using Microsoft.AspNetCore.Mvc;

using Database;
using MusicLibrary;

namespace API;

public static class LibraryEndpoints
{
    public record CreatePlaylistRequest(string PlaylistTitle, PlaylistType PlaylistType);
    public record UpdatePlaylistMetadataRequest(int PlaylistId, string? Title, string? Description, string? ImageFilePath);
    public record DeletePlaylistRequest(int PlaylistId);
    public record AddPlaylistTracksRequest(int PlaylistId, int[] TrackIds);
    public record RemovePlaylistTracksRequest(int PlaylistId, int[] Positions);
    public record MovePlaylistTracksRequest(int PlaylistId, int[] Positions, int TargetIndex);

    public static void MapLibraryEndpoints(this WebApplication app)
    {
        app.MapPost("/api/library", ([FromQuery] string path) =>
        {
            if(!Path.Exists(path) || !Directory.Exists(path))
                return Results.BadRequest("Invalid path.");
            
            Library.ImportFromPath(path);
            return Results.Ok();
        });
        
        app.MapGet("/api/library/albums", () =>
        {
            HashSet<Album> albums = Library.GetAlbums();
            return Results.Ok(albums);
        });

        app.MapGet("/api/library/albums/album", ([FromQuery] int albumId) =>
        {
            Console.WriteLine(albumId);

            if(albumId <= 0)
                return Results.BadRequest("Invalid albumId.");
            
            HashSet<Album> albums = Library.GetAlbums();
            Album? album = albums.FirstOrDefault(album => album.AlbumId == albumId);

            if(album is null)
                return Results.NotFound($"No album with albumId {albumId}.");
            return Results.Ok(albums);
        });

        app.MapGet("/api/library/playlists", () =>
        {
           List<Playlist> playlists = DatabaseManager.GetPlaylists();
           return Results.Ok(playlists); 
        });

        app.MapGet("/api/library/playlists/playlist", ([FromQuery] int playlistId) =>
        {
            if(playlistId <= 0)
                return Results.BadRequest("Invalid playlistId.");
            
            Playlist? playlist = DatabaseManager.GetPlaylistFromPlaylistId(playlistId);
            if(playlist is null)
                return Results.NotFound($"No playlist with playlistId {playlistId}.");

            return Results.Ok(playlist); 
        });

        app.MapPost("/api/library/playlists/playlist", (CreatePlaylistRequest request) =>
        {
            if(!Enum.IsDefined(request.PlaylistType))
                return Results.BadRequest("Invalid PlaylistType.");
            
            Playlist playlist = new Playlist(request.PlaylistTitle, request.PlaylistType);
            bool hasId = DatabaseManager.AddPlaylist(playlist);

            // if the playlist has an id, we can probably assume the playlist was succesfully saved to the database
            if(hasId)
                return Results.Created($"Successfully created playlist {request.PlaylistTitle}", playlist);
            else
                return Results.InternalServerError("Could not add playlist to database.");
        });

        app.MapPatch("/api/library/playlists/playlist", (UpdatePlaylistMetadataRequest request) =>
        {
            if(request.PlaylistId <= 0)
                return Results.BadRequest("Invalid playlistId.");
            
            if(!string.IsNullOrEmpty(request.ImageFilePath) && File.Exists(request.ImageFilePath))
                return Results.BadRequest("Invalid ImageFilePath.");
            
            Console.WriteLine($"ImageFilePath: {request.ImageFilePath}");

            Playlist? playlist = DatabaseManager.GetPlaylistFromPlaylistId(request.PlaylistId);
            if(playlist is null)
                return Results.NotFound($"No playlist with playlistId {request.PlaylistId}.");
            
            bool success = playlist.UpdateMetadata(request.Title, request.Description, request.ImageFilePath);
            if(success)
                return Results.Ok(playlist);
            else
                return Results.InternalServerError("Could not update metadata to database.");
        });

        app.MapDelete("/api/library/playlists/playlist", ([FromQuery] int playlistId) =>
        {
            if(playlistId <= 0)
                return Results.BadRequest("Invalid playlistId.");
            
            Playlist? playlist = DatabaseManager.GetPlaylistFromPlaylistId(playlistId);
            if(playlist is null)
                return Results.NotFound($"No playlist with playlistId {playlistId}.");
            
            bool success = DatabaseManager.DeletePlaylist(playlist);
            if(success)
                return Results.Ok();
            else
                return Results.InternalServerError("Could not delete playlist from database.");
        });

        app.MapPost("/api/library/playlists/playlist/tracks", (AddPlaylistTracksRequest request) =>
        {
            if(request.PlaylistId <= 0)
                return Results.BadRequest("Invalid playlistId.");
            
            Playlist? playlist = DatabaseManager.GetPlaylistFromPlaylistId(request.PlaylistId);
            if(playlist is null)
                return Results.NotFound($"No playlist with playlistId {request.PlaylistId}.");

            List<Track> tracks = Library.GetTracks()
                .Where(track => request.TrackIds.Contains(track.TrackId))
                .ToList();
            
            playlist.AddTracks(tracks);
            return Results.Ok(tracks);
        });

        app.MapPatch("/api/library/playlists/playlist/tracks", (MovePlaylistTracksRequest request) =>
        {
            if(request.PlaylistId <= 0)
                return Results.BadRequest("Invalid playlistId.");
            
            Playlist? playlist = DatabaseManager.GetPlaylistFromPlaylistId(request.PlaylistId);
            if(playlist is null)
                return Results.NotFound($"No playlist with playlistId {request.PlaylistId}.");
            
            if(request.TargetIndex >= playlist.Tracks.Count)
                return Results.BadRequest("Invalid targetIndex.");

            int[] validatedPositions = request.Positions
                .Distinct()
                .Where(position => position >= 0 && position < playlist.Tracks.Count)
                .ToArray();
            
            if(validatedPositions.Length == 0)
                return Results.NoContent();

            playlist.MoveTracks(validatedPositions, request.TargetIndex);
            return Results.Ok(playlist.Tracks);
        });

        app.MapDelete("/api/library/playlists/playlist/tracks", ([FromQuery] int playlistId, [FromQuery] int[] positions) =>
        {
            if(playlistId <= 0)
                return Results.BadRequest("Invalid playlistId.");
            
            Playlist? playlist = DatabaseManager.GetPlaylistFromPlaylistId(playlistId);
            if(playlist is null)
                return Results.NotFound($"No playlist with playlistId {playlistId}.");
            
            foreach(int pos in positions)
            {
                Console.Write(pos.ToString() + " ");
            }

            int[] validatedPositions = positions
                .Distinct()
                .Where(position => position >= 0 && position < playlist.Tracks.Count)
                .ToArray();
            
            foreach(int pos in validatedPositions)
            {
                Console.Write(pos.ToString() + " ");
            }

            if(validatedPositions.Length == 0)
                return Results.NoContent();

            playlist.RemoveTracks(validatedPositions);
            return Results.Ok(playlist.Tracks);
        });
    }
}