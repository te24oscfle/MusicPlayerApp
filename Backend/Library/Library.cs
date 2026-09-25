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
                List<string> albumTitles = DatabaseManager.GetAlbumTitles();
                List<string> newAlbumTitles = new List<string>();
                foreach(Track track in newTracks)
                {
                    if (albumTitles.Contains(track.Metadata.Album))
                        // TODO: Album already exists, add track to album
                        continue;
                    newAlbumTitles.Add(track.Metadata.Album);
                }

                // Create new albums
                List<Album> newAlbums = new List<Album>();
                foreach(string albumTitle in newAlbumTitles)
                {
                    List<Track> albumTracks = newTracks.Where(
                        track => 
                            track.Metadata.Album == albumTitle)
                    .ToList();
                }
                
                DatabaseManager.AddTracks(newTracks);

                // TODO: Insert tracks into database
            };
        }
    }
}