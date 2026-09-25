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

            if (tracks.Count == 0)
            {
                Console.WriteLine("The library is empty.");
                return Task.CompletedTask;
            }

            foreach(Track track in tracks)
            {
                TrackMetadata metadata = track.Metadata;
                Console.WriteLine($"{track.TrackId}. {metadata.Title}");
                Console.WriteLine($"\tTrack ID: {track.TrackId}");
                Console.WriteLine($"\tAlbum ID: {track.AlbumId}");
                Console.WriteLine($"\tFile Path: {track.FilePath}");
                Console.Write("\n");
                Console.WriteLine($"\tTitle: {metadata.Title}");
                Console.WriteLine($"\tAlbum: {metadata.Album}");
                Console.WriteLine($"\tAlbum Artist: {metadata.AlbumArtist}");
                Console.WriteLine($"\tTrack Number: {metadata.TrackNumber}");
                Console.WriteLine($"\tDisc Number: {metadata.DiscNumber}");
                Console.WriteLine($"\tDuration: {metadata.DurationSeconds} seconds");
                Console.WriteLine($"\tDate: {metadata.Date}");
                Console.WriteLine($"\tGenre: {metadata.Genre}");
                Console.Write("\n");
                Console.Write("\n");
            }

            return Task.CompletedTask;
        }
    }
}