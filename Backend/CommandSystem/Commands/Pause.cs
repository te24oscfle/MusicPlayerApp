using MusicPlayerApp;
using Playback;

namespace CommandSystem
{
    [Command(
        "pause", 
        "Pauses playback", 
        "pause"
    )]
    public class Pause : ICommand
    {
        public Task Execute(string[] arguments)
        {
            AudioPlayer.Pause();
            return Task.CompletedTask;
        }
    }
}