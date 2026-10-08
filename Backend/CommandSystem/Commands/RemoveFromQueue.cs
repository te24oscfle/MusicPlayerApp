using Playback;

namespace CommandSystem
{
    [Command(
        "removefromqueue", 
        "Removes all tracks provided from the queue", 
        "removefromqueue <index...>"
    )]
    public class RemoveFromQueue : ICommand
    {
        public Task Execute(string[] arguments)
        {
            int[] indexes = arguments
                .Select(trackIdString =>
                {
                    int.TryParse(trackIdString, out int trackId);
                    return trackId;
                })
                .OfType<int>()
                .ToArray();

            QueueManager.RemoveTracksFromQueue(indexes);

            return Task.CompletedTask;
        }
    }
}