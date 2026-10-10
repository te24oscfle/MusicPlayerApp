using Playback;

namespace CommandSystem
{
    [Command(
        "next", 
        "Advances to the next track in the queue", 
        "next"
    )]
    public class Next : ICommand
    {
        public Task Execute(string[] arguments)
        {
            PlaybackResult playbackResult = QueueManager.Next();
            AudioPlayer.PlayTrackFromPlaybackResult(playbackResult);
            return Task.CompletedTask;
        }
    }
}