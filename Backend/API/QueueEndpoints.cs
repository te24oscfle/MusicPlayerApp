using Database;
using Microsoft.AspNetCore.Mvc;

using MusicLibrary;
using Playback;

namespace API;

public static class QueueEndpoints
{
    public record MoveQueueTracksRequest(int[] Positions, int TargetIndex);

    public static void MapQueueEndpoints(this WebApplication app)
    {
        app.MapGet("/api/queue/tracks", () =>
        {
            return Results.Ok(new
            {
                QueueManager.Queue,
                QueueManager.CurrentIndex
            });
        });
        
        app.MapPost("/api/queue/tracks", ([FromBody] int[] trackIds) =>
        {
            if(trackIds is null || trackIds.Length == 0)
                return Results.BadRequest("No trackIds provided");
            
            List<Track> tracks = trackIds
                .Distinct()
                .Where(id => id > 0)
                .Select(id => Library.GetTrackFromTrackId(id)!)
                .Where(track => track is not null)
                .ToList();
            
            if(tracks.Count == 0)
                return Results.NoContent();
            
            QueueManager.AddTracksToQueue(tracks, false, true);
            return Results.Ok(QueueManager.Queue);
        });

        app.MapPatch("/api/queue/tracks", (MoveQueueTracksRequest request) =>
        {
            if(request.Positions is null || request.Positions.Length == 0)
                return Results.BadRequest("No positions provided.");
            
            if(request.TargetIndex < 0 || request.TargetIndex >= QueueManager.Queue.Count)
                return Results.BadRequest("Invalid targetIndex.");
            
            int[] validatedPositions = request.Positions
                .Distinct()
                .Where(position => position >= 0 && position < QueueManager.Queue.Count)
                .ToArray();
            
            if(validatedPositions.Length == 0)
                return Results.NoContent();
            
            QueueManager.MoveTracks(validatedPositions, request.TargetIndex);
            return Results.Ok(QueueManager.Queue);
        });

        app.MapDelete("/api/queue/tracks", ([FromQuery] int[] positions) =>
        {
            if(positions is null || positions.Length == 0)
                return Results.BadRequest("No positions provided.");
            
            int[] validatedPositions = positions
                .Distinct()
                .Where(position => position >= 0 && position < QueueManager.Queue.Count)
                .ToArray();
            
            if(validatedPositions.Length == 0)
                return Results.NoContent();

            QueueManager.RemoveTracksFromQueue(validatedPositions);
            return Results.Ok(QueueManager.Queue);
        });

        app.MapDelete("/api/queue", () =>
        {
            QueueManager.ClearQueue();
            return Results.Ok();
        });

        app.MapPatch("/api/queue/loopState", ([FromQuery] LoopState loopState) =>
        {
            if(!Enum.IsDefined(loopState))
                return Results.BadRequest("Invalid loopState.");
            
            QueueManager.SetLoopState(loopState);
            return Results.Ok(QueueManager.LoopState.ToString());
        });

        app.MapPatch("/api/queue/shuffleState", ([FromQuery] ShuffleState shuffleState) =>
        {
            if(!Enum.IsDefined(shuffleState))
                return Results.BadRequest("Invalid shuffleState.");
            
            QueueManager.SetShuffleState(shuffleState);
            return Results.Ok(QueueManager.ShuffleState.ToString());
        });

        // TODO: Add int StartIndex and start playing album from StartIndex. if startIndex is invalid, play from 0.
        app.MapPatch("/api/queue/album", ([FromQuery] int albumId, int startIndex) =>
        {
            if(albumId <= 0)
                return Results.BadRequest("Invalid albumId.");
            
            Album? album = Library.GetAlbumFromAlbumId(albumId);
            if(album is null)
                return Results.NotFound($"No album with albumId {albumId}");
            
            QueueManager.PlayAlbum(album, startIndex);
            return Results.Ok();
        });

        // TODO: Add int StartIndex and start playing album from StartIndex. if startIndex is invalid, play from 0.
        app.MapPatch("/api/queue/playlist", ([FromQuery] int playlistId, [FromQuery] int startIndex) =>
        {
            if(playlistId <= 0)
                return Results.BadRequest("Invalid playlistId.");
            
            Playlist? playlist = DatabaseManager.GetPlaylistFromPlaylistId(playlistId);
            if(playlist is null)
                return Results.NotFound($"No album with playlistId {playlistId}");

            QueueManager.PlayPlaylist(playlist, startIndex);
            return Results.Ok();
        });
    }
}