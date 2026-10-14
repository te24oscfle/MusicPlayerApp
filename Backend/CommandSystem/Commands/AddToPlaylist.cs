using Database;
using MusicPlayerApp;

namespace CommandSystem
{
    [Command(
        "addToPlaylist", 
        "Adds tracks to provided playlist", 
        "addToPlaylist <playlistId> <trackId...>"
    )]
    public class AddToPlaylist : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if (arguments.Length < 2)
            {
                Console.WriteLine("Not enough arguments");
                return Task.CompletedTask;
            }
            
            int.TryParse(arguments[0], out int playlistId);
            List<Track> tracks = arguments.Skip(1)
                .Select(int.Parse)
                .Select(trackId => Library.GetTrackFromTrackId(trackId))
                .OfType<Track>()
                .ToList();

            Playlist? playlist = DatabaseManager.GetPlaylistFromPlaylistId(playlistId);
            if (playlist is null)
            {
                Console.WriteLine($"Could not find playlist with playlistId={playlistId}");
                return Task.CompletedTask;
            }

            playlist.AddTracks(tracks);
            return Task.CompletedTask;
        }
    }
}