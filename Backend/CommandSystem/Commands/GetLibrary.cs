using Database;
using MusicPlayerApp;

namespace CommandSystem
{
    [Command(
        "getlibrary", 
        "Lists all tracks in the library", 
        "getlibrary"
    )]
    public class GetLibrary : ICommand
    {
        public Task Execute(string[] arguments)
        {
            List<Track> tracks = DatabaseManager.GetTracks();
            foreach(Track track in tracks)
                Console.WriteLine($"ID={track.TrackId}: {track.FilePath}");
            return Task.CompletedTask;
        }
    }
}