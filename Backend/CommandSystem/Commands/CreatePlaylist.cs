using MusicPlayerApp;
using Database;

namespace CommandSystem
{
    [Command(
        "createPlaylist", 
        "Creates a new playlist", 
        "createPlaylist <title?>"
    )]
    public class CreatePlaylist : ICommand
    {
        public Task Execute(string[] arguments)
        {
            string title = string.Join(" ", arguments);
            Playlist newPlaylist = new Playlist(!string.IsNullOrEmpty(title) ? title : "My playlist", PlaylistType.Manual);

            DatabaseManager.AddPlaylist(newPlaylist);
            
            Console.WriteLine($"Created new playlist '{newPlaylist.Title}' (PlaylistId={newPlaylist.PlaylistId})");
            return Task.CompletedTask;
        }
    }
}