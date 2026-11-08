using Microsoft.AspNetCore.Mvc;

using MusicLibrary;
using Playback;

namespace API;

public static class QueueEndpoints
{
    public static void MapQueueEndpoints(this WebApplication app)
    {
        app.MapPost("/api/queue/tracks", ([FromQuery] int[] trackIds) =>
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
    }
}