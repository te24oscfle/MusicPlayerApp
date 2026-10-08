using Playback;

namespace CommandSystem
{
    [Command(
        "movetrackinqueue", 
        "Moves track at fromIndex in the queue to toIndex", 
        "movetrackinqueue <fromIndex> <toIndex>"
    )]
    public class MoveTrackInQueue : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if (arguments.Length < 2)
            {
                Console.WriteLine("Not enough arguments");
                return Task.CompletedTask;
            }
            
            int.TryParse(arguments[0], out int fromIndex);
            int.TryParse(arguments[1], out int toIndex);

            QueueManager.MoveTrack(fromIndex, toIndex);

            return Task.CompletedTask;
        }
    }
}