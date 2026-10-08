using MusicPlayerApp;
using Playback;

namespace CommandSystem
{
    [Command(
        "addToQueue", 
        "Adds all tracks provided to the queue", 
        "addToQueue <trackId...>"
    )]
    public class AddToQueue : ICommand
    {
        public Task Execute(string[] arguments)
        {
            List<Track> tracks = arguments
                .Select(trackIdString =>
                {
                    int.TryParse(trackIdString, out int trackId);
                    return trackId;
                })
                .OfType<int>()
                .Select(trackId => Library.GetTrackFromTrackId(trackId))
                .OfType<Track>()
                .ToList();

            QueueManager.AddTracksToQueue(tracks, false, true);

            return Task.CompletedTask;
        }
    }
}