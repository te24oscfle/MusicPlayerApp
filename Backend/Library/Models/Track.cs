namespace MusicPlayerApp
{
    public class Track
    {
        public int TrackId;
        public string FilePath;

        public Track(int trackId, string filePath)
        {
            TrackId = trackId;
            FilePath = filePath;
        }
    }
}