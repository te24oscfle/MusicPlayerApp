using Database;
using MusicPlayerApp;

namespace CommandSystem
{
    [Command(
        "removeFromPlaylist", 
        "Removes tracks from provided playlist", 
        "removeFromPlaylist <playlistId> <positions...>"
    )]
    public class RemoveFromPlaylist : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if (arguments.Length < 2)
            {
                Console.WriteLine("Not enough arguments");
                return Task.CompletedTask;
            }
            
            int.TryParse(arguments[0], out int playlistId);
            int[] positions = arguments.Skip(1)
                .Select(int.Parse)
                .OfType<int>()
                .ToArray();

            Playlist? playlist = DatabaseManager.GetPlaylistFromPlaylistId(playlistId);
            if (playlist is null)
            {
                Console.WriteLine($"Could not find playlist with playlistId={playlistId}");
                return Task.CompletedTask;
            }

            playlist.RemoveTracks(positions);
            return Task.CompletedTask;
        }
    }
}