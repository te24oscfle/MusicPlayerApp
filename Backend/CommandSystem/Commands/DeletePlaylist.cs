using MusicPlayerApp;
using Database;

namespace CommandSystem
{
    [Command(
        "deletePlaylist", 
        "Deletes the provided playlist", 
        "deletePlaylist <playlistId>"
    )]
    public class DeletePlaylist : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if(arguments.Length == 0)
            {
                Console.WriteLine("No PlaylistID was provided");
                return Task.CompletedTask;
            }

            int.TryParse(arguments[0], out int playlistId);
            Playlist? playlist = DatabaseManager.GetPlaylistFromPlaylistId(playlistId);
            if(playlist is null)
            {
                Console.WriteLine($"Could not find playlist with PlaylistId={playlistId}");
                return Task.CompletedTask;
            }
            
            DatabaseManager.DeletePlaylist(playlist);
            return Task.CompletedTask;
        }
    }
}