using MusicPlayerApp;
using Playback;

namespace CommandSystem
{
    [Command(
        "play", 
        "Plays a track. If no trackId is provided, the command will attempt to resume playback", 
        "play <trackId?>"
    )]
    public class Play : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if (arguments.Length == 0)
            {
                AudioPlayer.Resume();
                return Task.CompletedTask;
            }
            
            int.TryParse(arguments[0], out int trackId);

            Track? track = Library.GetTrackFromTrackId(trackId);
            if (track == null)
            {
                Console.WriteLine($"Could not find Track with TrackId={trackId}");
                return Task.CompletedTask;
            }

            AudioPlayer.PlayTrack(track);

            return Task.CompletedTask;
        }
    }
}