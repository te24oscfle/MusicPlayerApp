using System.Security.Cryptography.X509Certificates;
using System.Threading.RateLimiting;
using ATL;
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
        
        private static Dictionary<int, Album> albumCache = new Dictionary<int, Album>();
        private static Dictionary<int, Track> trackCache = new Dictionary<int, Track>();

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

        public static void LoadLibraryFromDatabase()
        {
            trackCache.Clear();
            albumCache.Clear();
            
            List<Track> tracks = DatabaseManager.GetTracks();
            foreach(Track track in tracks)
            {
                trackCache.Add(track.TrackId, track);
            }

            List<Album> albums = DatabaseManager.GetAlbumsFromTracks(tracks);
            foreach(Album album in albums)
            {
                albumCache.Add(album.AlbumId, album);
            }
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
                HashSet<string> filePathsInDatabase = DatabaseManager.GetFilePaths().ToHashSet();

                // Create track objects
                List<Track> newTracks = new List<Track>(validFilePaths.Length);

                foreach(string filePath in validFilePaths)
                {
                    if (filePathsInDatabase.Contains(filePath))
                        continue;
                    newTracks.Add(new Track(filePath));
                }

                // Find new albums
                HashSet<string> albumKeys = DatabaseManager.GetAlbumKeys().ToHashSet();
                HashSet<string> newAlbumKeys = new List<string>().ToHashSet();
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

                    // If albumKey already exists, nothing will happen as HashSets do not allow for dupliacte values
                    newAlbumKeys.Add(albumKey);
                }

                // Create new album covers
                Dictionary<string, AlbumCover> newAlbumCovers = new Dictionary<string, AlbumCover>();
                
                foreach(Track track in newTracks)
                {
                    ATL.Track metadata = new ATL.Track(track.FilePath);
                    foreach(PictureInfo pictureInfo in metadata.EmbeddedPictures)
                    {
                        // TODO: Add more robust system to set priority to certain types of pictures.
                        if(pictureInfo.PicType != PictureInfo.PIC_TYPE.Front)
                            continue;
                        
                        // Get Album cover hash to prevent duplicate album cover entries
                        string hash = AlbumCover.GetHashFromImageData(pictureInfo.PictureData);
                        AlbumCover albumCover;
                        if(!newAlbumCovers.ContainsKey(hash))
                        {
                            // New album cover, add to list   
                            albumCover = new AlbumCover(pictureInfo);
                            newAlbumCovers.Add(hash, albumCover);
                        } 
                        else
                        {
                            // This album cover already exists
                            albumCover = newAlbumCovers[hash];
                        }

                        // Initilize track with album cover
                        try
                        {
                            track.InitilizeAlbumCover(albumCover);
                        } catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        
                    }
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

                    AlbumCover? albumCover = album.GetTracks().FirstOrDefault()!.Metadata.AlbumCover;
                    if(albumCover is not null)
                    {
                       album.InitilizeAlbumCover(albumCover); 
                    }
                    
                    newAlbums.Add(album);
                }

                // Add album covers to database
                DatabaseManager.AddAlbumCovers(newAlbumCovers.Values.ToList());
                
                // Add albums to database, which also adds each track in respective album
                foreach(Album album in newAlbums)
                {
                    DatabaseManager.AddAlbum(album);
                }

                // Add tracks without album to database
                DatabaseManager.AddTracks(
                    newTracks.Where(
                        track => 
                            track.AlbumId == 0)
                        .ToList()
                    );
                
                // Update track and album cache
                LoadLibraryFromDatabase();
            };
        }

        public static Track? GetTrackFromTrackId(int trackId)
        {
            trackCache.TryGetValue(trackId, out Track? track);
            return track;
        }

        public static Album? GetAlbumFromAlbumId(int albumId)
        {
            albumCache.TryGetValue(albumId, out Album? album);
            return album;
        }

        public static HashSet<Album> GetAlbums()
        {
            return albumCache.Values.ToHashSet();
        }

        public static HashSet<Track> GetTracks()
        {
            return trackCache.Values.ToHashSet();
        }
    }
}