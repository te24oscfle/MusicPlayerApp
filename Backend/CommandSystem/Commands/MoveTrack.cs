using Database;
using MusicPlayerApp;
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
            
            int.TryParse(arguments[0], out int playlistId);
            int.TryParse(arguments[1], out int toIndex);
            int[] positions = arguments.Skip(2).Select(int.Parse).ToArray();

            if(playlistId > 0)
            {
                Playlist? playlist = DatabaseManager.GetPlaylistFromPlaylistId(playlistId);
                if (playlist is null)
                {
                    Console.WriteLine($"Could not find playlist with playlistId={playlistId}");
                    return Task.CompletedTask;
                }

                playlist.MoveTracks(positions, toIndex);
                return Task.CompletedTask;
            }
            // TODO: Fix QueueManager.MoveTracks
            QueueManager.MoveTrack(positions[0], toIndex);

            return Task.CompletedTask;
        }
    }
}