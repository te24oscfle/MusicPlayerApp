using Playback;

namespace CommandSystem
{
    [Command(
        "getQueue", 
        "Lists all tracks in the queue", 
        "getQueue"
    )]
    public class GetQueue : ICommand
    {
        public Task Execute(string[] arguments)
        {
            List<QueueItem> queue = QueueManager.Queue;
            
            if (queue.Count == 0)
            {
                Console.WriteLine("The queue is empty.");
                return Task.CompletedTask;
            }

            Console.WriteLine("Current Queue:");
            for(int i = 0; i < queue.Count; i++)
            {
                QueueItem queueItem = queue[i];
                Console.WriteLine($"\t{i}. {queueItem.Track.Metadata.Title}");
            }
            Console.Write("\n");

            return Task.CompletedTask;
        }
    }
}