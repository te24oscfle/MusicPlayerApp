using System.Reflection.Metadata.Ecma335;

namespace MusicPlayerApp
{
    public class TrackMetadata
    {
        public string Title { get; }
        public string Artist { get; }
        public string Album { get; }
        public string AlbumArtist { get; }
        public int TrackNumber { get; }
        public int DiscNumber { get; }
        public int DurationSeconds { get; }
        public DateTime? Date { get; }
        public string Genre { get; }

        public TrackMetadata(
            string title,
            string artist,
            string album,
            string albumArtist,
            int trackNumber,
            int discNumber,
            int durationSeconds,
            DateTime? date,
            string genre
        )
        {
            Title = title;
            Artist = artist;
            Album = album;
            AlbumArtist = albumArtist;
            TrackNumber = trackNumber;
            DiscNumber = discNumber;
            DurationSeconds = durationSeconds;
            Date = date;
            Genre = genre;
        }
        public TrackMetadata(ATL.Track atlTrack) : this (
                atlTrack.Title,
                atlTrack.Artist,
                atlTrack.Album,
                atlTrack.AlbumArtist,
                atlTrack.TrackNumber ?? 0,
                atlTrack.DiscNumber ?? 1,
                atlTrack.Duration,
                atlTrack.Date,
                atlTrack.Genre
            ) {}

    };
    
    public class Track
    {
        public int TrackId;
        public string FilePath;
        public int AlbumId;
        public TrackMetadata Metadata;

        public Track(int trackId, string filePath, int albumId, TrackMetadata metadata)
        {
            TrackId = trackId;
            FilePath = filePath;
            AlbumId = albumId;
            Metadata = metadata;
        }

        public Track(string filePath) 
            : this(0, filePath, 0, new TrackMetadata(new ATL.Track(filePath))) {}

        public Track(int trackId, string filePath) 
            : this(trackId, filePath, 0, new TrackMetadata(new ATL.Track(filePath))) {}

        public override bool Equals(object? obj)
        {
            if (obj is not Track track)
                return false;

            return TrackId == track.TrackId;
        }
    }
}