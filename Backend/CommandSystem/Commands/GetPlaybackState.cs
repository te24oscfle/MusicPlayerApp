using MusicPlayerApp;
using Playback;

namespace CommandSystem
{
    [Command(
        "getPlaybackState", 
        "Prints out current track, IsPlaying, CurrentPosition, and CurrentVolume", 
        "getPlaybackState"
    )]
    public class GetPlaybackState : ICommand
    {
        public Task Execute(string[] arguments)
        {
            PlaybackState playbackState = AudioPlayer.GetPlaybackState();

            string currentTrackString = playbackState.CurrentTrack is not null 
                ? playbackState.CurrentTrack.Metadata.Title 
                : "None";

            Console.WriteLine($"Current Track: {currentTrackString}");
            Console.WriteLine($"IsPlaying: {playbackState.IsPlaying}");
            Console.WriteLine($"Current Position: {playbackState.Position}");
            Console.WriteLine($"Current Volume: {playbackState.Volume}");

            return Task.CompletedTask;
        }
    }
}