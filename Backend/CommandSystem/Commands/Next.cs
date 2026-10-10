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
            QueueManager.Next();
            return Task.CompletedTask;
        }
    }
}