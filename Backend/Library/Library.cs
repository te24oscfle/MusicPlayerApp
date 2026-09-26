using System.Threading.RateLimiting;
using Database;

namespace MusicPlayerApp
{
    public static class Library
    {
        public static string[] SUPPORTED_FORMATS =
        {
            ".mp3",
            ".wav",
            ".flac"
        };
        
        private static string[] GetFilePathsFromDirectory(string directoryPath)
        {
            string[] filePaths = Directory.GetFiles(directoryPath);
            string[] subdirectoryPaths = Directory.GetDirectories(directoryPath);
            foreach(string path in subdirectoryPaths)
            {
                string[] paths = GetFilePathsFromDirectory(path);
                filePaths = filePaths.Concat(paths).ToArray();
            }
            return filePaths;
        }
        
        // Filter out file paths not ending with .mp3, .wav, 
        // or other supported audio formats, see SUPPORTED_FORMATS
        private static string[] GetValidFilePaths(string[] filePaths)
        {
            return filePaths
                .Where(path => SUPPORTED_FORMATS.Contains(Path.GetExtension(path)))
                .ToArray();
        }
        
        public static string SerializeAlbumKey(Track track)
        {
            TrackMetadata metadata = track.Metadata;
            string albumTitle = metadata.Album ?? "__unknown__";
            string albumArtist = metadata.AlbumArtist ?? metadata.Artist ?? "__unknown__";
            return string.Join("///", albumTitle, albumArtist);
        }
        public static string SerializeAlbumKey(string? albumTitle, string? albumArtist)
        {
            return string.Join("///", albumTitle ?? "__unknown__", albumArtist ?? "__unknown__");
        }

        public static (string albumTitle, string albumArtist) DeserializeAlbumKey(string albumKey)
        {
            string[] parts = albumKey.Split("///");
            Console.WriteLine(albumKey);
            string albumTitle = parts[0];
            string albumArtist = parts[1];
            return (albumTitle, albumArtist);
        }

        public static void ImportFromPath(string path)
        {
            if (!Path.Exists(path))
                throw new ArgumentException($"Invalid path {path}");
            
            if(Directory.Exists(path))
            {
                // Get all valid filePaths
                string[] filePaths = GetFilePathsFromDirectory(path);
                string[] validFilePaths = GetValidFilePaths(filePaths);

                // Get file paths already in database
                List<string> filePathsInDatabase = DatabaseManager.GetFilePaths();

                // Create track objects
                List<Track> newTracks = new List<Track>(validFilePaths.Length);

                foreach(string filePath in validFilePaths)
                {
                    if (filePathsInDatabase.Contains(filePath))
                        continue;
                    newTracks.Add(new Track(filePath));
                }

                // Find new albums
                List<string> albumKeys = DatabaseManager.GetAlbumKeys();
                List<string> newAlbumKeys = new List<string>();
                foreach(Track track in newTracks)
                {
                    string albumKey = SerializeAlbumKey(track);
                    if (albumKeys.Contains(albumKey))
                    {
                        // Album already exists, add track to album
                        (string albumTitle, string albumArtist) = DeserializeAlbumKey(albumKey);
                        int albumId = DatabaseManager.GetAlbumIdFromAlbum(albumTitle, albumArtist);
                        track.AlbumId = albumId;
                        continue;
                    }

                    // Already found this album                    
                    if (newAlbumKeys.Contains(albumKey))
                        continue;
                    
                    newAlbumKeys.Add(albumKey);
                }

                // Create new albums
                List<Album> newAlbums = new List<Album>();
                foreach(string albumKey in newAlbumKeys)
                {
                    List<Track> albumTracks = newTracks.Where(
                        track =>
                            SerializeAlbumKey(track) == albumKey)
                    .ToList();
                    
                    (string albumTitle, string albumArtist) = DeserializeAlbumKey(albumKey);
                    Album album = new Album(albumTitle, albumArtist);
                    album.AddTracks(albumTracks);

                    newAlbums.Add(album);
                }
                
                foreach(Album album in newAlbums)
                {
                    // Console.WriteLine("============================================");
                    // Console.Write("\n");
                    // Console.WriteLine($"{album.Title} - {album.Artist}");
                    // Console.WriteLine($"{album.Discs.Count} discs");
                    // Console.Write("\n");

                    // foreach(var pair in album.Discs)
                    // {
                    //     Console.WriteLine($"Disc {pair.Key}");
                    //     foreach(Track track in pair.Value)
                    //     {
                    //         Console.WriteLine($"\t{track.Metadata.TrackNumber}. {track.Metadata.Title}");
                    //     }
                    //     Console.Write("\n");
                    // }
                    // Console.Write("\n");

                    DatabaseManager.AddAlbum(album);
                }

                DatabaseManager.AddTracks(
                    newTracks.Where(
                        track => 
                            track.AlbumId == 0)
                        .ToList()
                    );
            };
        }
    }
}