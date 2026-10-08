using Playback;

namespace CommandSystem
{
    [Command(
        "clearQueue", 
        "Clears the queue", 
        "clearQueue"
    )]
    public class ClearQueue : ICommand
    {
        public Task Execute(string[] arguments)
        {
            QueueManager.ClearQueue();
            return Task.CompletedTask;
        }
    }
}