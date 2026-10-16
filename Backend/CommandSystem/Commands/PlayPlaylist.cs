using Database;
using MusicPlayerApp;
using Playback;

namespace CommandSystem
{
    [Command(
        "playPlaylist", 
        "Sets the queue to playlist with corresponding playlistId", 
        "playPlaylist <playlistId>"
    )]
    public class PlayPlaylist : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                Console.WriteLine("No album ID provided");
                return Task.CompletedTask;
            }

            int.TryParse(arguments[0], out int playlistId);
            Playlist? playlist = DatabaseManager.GetPlaylistFromPlaylistId(playlistId);
            if (playlist is null)
            {
                Console.WriteLine($"Could not find playlist with playlistId={playlistId}");
                return Task.CompletedTask;
            }

            QueueManager.PlayPlaylist(playlist);

            return Task.CompletedTask;
        }
    }
}