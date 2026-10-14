using Database;
using MusicPlayerApp;

namespace CommandSystem
{
    [Command(
        "setPlaylistTitle", 
        "Sets the title of provided playlist", 
        "setPlaylistTitle <playlistId> <title>"
    )]
    public class SetPlaylistTitle : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if(arguments.Length < 2)
            {
                Console.WriteLine("Not enough arguments provided");
                return Task.CompletedTask;
            }

            int.TryParse(arguments[0], out int playlistId);
            string title = string.Join(" ", arguments.Skip(1));

            Playlist? playlist = DatabaseManager.GetPlaylistFromPlaylistId(playlistId);
            if(playlist is null)
            {
                Console.WriteLine($"Could not find playlist with playlistId={playlistId}");
                return Task.CompletedTask;
            }
            
            string oldTitle = playlist.Title;

            playlist.UpdateMetadata(title, null, null);
            Console.WriteLine($"Updated title of playlist {oldTitle} to {title}");
            return Task.CompletedTask;
        }
    }

    [Command(
        "setPlaylistDescription", 
        "Sets the description of provided playlist", 
        "setPlaylistDescription <playlistId> <description>"
    )]
    public class SetPlaylistDescription : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if(arguments.Length < 2)
            {
                Console.WriteLine("Not enough arguments provided");
                return Task.CompletedTask;
            }

            int.TryParse(arguments[0], out int playlistId);
            string description = string.Join(" ", arguments.Skip(1));

            Playlist? playlist = DatabaseManager.GetPlaylistFromPlaylistId(playlistId);
            if(playlist is null)
            {
                Console.WriteLine($"Could not find playlist with playlistId={playlistId}");
                return Task.CompletedTask;
            }

            playlist.UpdateMetadata(null, description, null);
            Console.WriteLine($"Updated description for playlist {playlist.Title}");
            return Task.CompletedTask;
        }
    }

    [Command(
        "setPlaylistImage", 
        "Sets the image file path of provided playlist", 
        "setPlaylistImage <playlistId> <imageFilePath>"
    )]
    public class SetPlaylistImage : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if(arguments.Length < 2)
            {
                Console.WriteLine("Not enough arguments provided");
                return Task.CompletedTask;
            }

            int.TryParse(arguments[0], out int playlistId);
            string imageFilePath = string.Join(" ", arguments.Skip(1));

            Playlist? playlist = DatabaseManager.GetPlaylistFromPlaylistId(playlistId);
            if(playlist is null)
            {
                Console.WriteLine($"Could not find playlist with playlistId={playlistId}");
                return Task.CompletedTask;
            }

            playlist.UpdateMetadata(null, null, imageFilePath);
            Console.WriteLine($"Updated image file path for playlist {playlist.Title}");
            return Task.CompletedTask;
        }
    }
}