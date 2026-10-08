using Playback;

namespace CommandSystem
{
    [Command(
        "clearqueue", 
        "Clears the queue", 
        "clearqueue"
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