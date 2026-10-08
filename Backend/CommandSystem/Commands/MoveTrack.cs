using Playback;

namespace CommandSystem
{
    [Command(
        "moveTrack", 
        "Moves track at fromIndex in the queue to toIndex in corresponding playlist. PlaylistId of 0 is equilivent to the queue", 
        "moveTrack <playlistId> <fromIndex> <toIndex>"
    )]
    public class MoveTrack : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if (arguments.Length < 3)
            {
                Console.WriteLine("Not enough arguments");
                return Task.CompletedTask;
            }
            
            int.TryParse(arguments[1], out int playlistId);
            int.TryParse(arguments[1], out int fromIndex);
            int.TryParse(arguments[2], out int toIndex);

            // TODO: Move track in playlist is playlistId is greater than 0
            QueueManager.MoveTrack(fromIndex, toIndex);

            return Task.CompletedTask;
        }
    }
}