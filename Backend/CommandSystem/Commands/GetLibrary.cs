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
            Library.LoadLibraryFromDatabase();
            HashSet<Album> albums = Library.GetAlbums();

            if (albums.Count == 0)
            {
                Console.WriteLine("The library is empty.");
                return Task.CompletedTask;
            }

            foreach(Album album in albums)
            {
                Console.WriteLine("============================================");
                Console.Write("\n");
                Console.WriteLine($"{album.AlbumId}. {album.Title} - {album.Artist}");
                Console.WriteLine($"{album.Discs.Count} discs");
                Console.Write("\n");

                foreach(var pair in album.Discs)
                {
                    Console.WriteLine($"Disc {pair.Key}");
                    foreach(Track track in pair.Value)
                    {
                        Console.WriteLine($"\tTrack Id: {track.TrackId}. Track Number: {track.Metadata.TrackNumber}. {track.Metadata.Title}");
                    }
                    Console.Write("\n");
                }
                Console.Write("\n");
            }

            return Task.CompletedTask;
        }
    }
}