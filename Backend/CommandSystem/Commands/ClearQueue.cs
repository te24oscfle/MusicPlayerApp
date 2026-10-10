using Playback;

namespace CommandSystem
{
    [Command(
        "clearQueue", 
        "Clears non persisntent tracks in the queue", 
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

    [Command(
        "forceClearQueue", 
        "Clears all the tracks in the queue", 
        "forceClearQueue"
    )]
    public class ForceClearQueue : ICommand
    {
        public Task Execute(string[] arguments)
        {
            QueueManager.ClearQueue(true);
            return Task.CompletedTask;
        }
    }
}