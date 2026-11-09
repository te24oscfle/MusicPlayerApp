using Microsoft.AspNetCore.Mvc;

using MusicLibrary;
using Playback;

namespace API;

public static class PlaybackEndpoints
{
    public static void MapPlaybackEndpoints(this WebApplication app)
    {
        app.MapGet("/api/playback", () =>
        {
            return Results.Ok(AudioPlayer.GetPlaybackState());
        });

        app.MapPost("/api/playback/play", () =>
        {
            AudioPlayer.Resume();
            return Results.NoContent();
        });

        app.MapPost("/api/playback/pause", () =>
        {
            AudioPlayer.Pause();
            return Results.NoContent();
        });

        app.MapPost("/api/playback/next", () =>
        {
            PlaybackResult result = QueueManager.Next();
            AudioPlayer.PlayTrackFromPlaybackResult(result);
            return Results.NoContent();
        });

        app.MapPost("/api/playback/previous", () =>
        {
            PlaybackResult result = QueueManager.Previous();
            AudioPlayer.PlayTrackFromPlaybackResult(result);
            return Results.NoContent();
        });

        app.MapPatch("/api/playback/volume", ([FromQuery] int volume) => {
            int clampedVolume = Math.Clamp(volume, 0, 100);
            AudioPlayer.SetVolume(clampedVolume);
            return Results.NoContent();
        });

        app.MapPatch("/api/playback/seek", ([FromQuery] int positionInSeconds) =>
        {
            AudioPlayer.Seek(positionInSeconds);
            return Results.NoContent();
        });
    }
}