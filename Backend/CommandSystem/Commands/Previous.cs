using Playback;

namespace CommandSystem
{
    [Command(
        "previous", 
        "Advances to the previous track in the queue", 
        "next"
    )]
    public class Previous : ICommand
    {
        public Task Execute(string[] arguments)
        {
            QueueManager.Previous();
            return Task.CompletedTask;
        }
    }
}