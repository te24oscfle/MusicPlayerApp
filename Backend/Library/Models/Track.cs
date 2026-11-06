using System.Globalization;

using Database;

namespace MusicLibrary;

public class TrackMetadata
{
    public string Title { get; }
    public string Artist { get; }
    public string Album { get; }
    public string AlbumArtist { get; }
    public AlbumCover? AlbumCover { get; internal set; }
    public int TrackNumber { get; }
    public int DiscNumber { get; }
    public int DurationSeconds { get; }
    public DateTime Date { get; }
    public string Genre { get; }
    public DateTime LastModifiedUtc { get; }

    public TrackMetadata(
        string title,
        string artist,
        string album,
        string albumArtist,
        int trackNumber,
        int discNumber,
        int durationSeconds,
        DateTime date,
        string genre,
        DateTime lastModifiedUtc
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
        LastModifiedUtc = lastModifiedUtc;
    }
    public TrackMetadata(ATL.Track atlTrack) : this (
            string.IsNullOrWhiteSpace(atlTrack.Title) ? Path.GetFileNameWithoutExtension(atlTrack.Path) : atlTrack.Title,
            string.IsNullOrWhiteSpace(atlTrack.Artist) ? "__unknown__" : atlTrack.Artist,
            string.IsNullOrWhiteSpace(atlTrack.Album) ? "__unknown__" : atlTrack.Album,
            string.IsNullOrWhiteSpace(atlTrack.AlbumArtist) ? "__unknown__" : atlTrack.AlbumArtist,
            atlTrack.TrackNumber ?? 0,
            atlTrack.DiscNumber is > 0 ? atlTrack.DiscNumber.Value : 1,
            atlTrack.Duration,
            atlTrack.Date ?? new DateTime(0),
            atlTrack.Genre,
            File.GetLastWriteTimeUtc(atlTrack.Path)
        ) {}
};

public class Track
{
    public int TrackId { get; set; }
    public string FilePath { get; }
    public int AlbumId { get; set; }
    public TrackMetadata Metadata { get; }
    public bool ShouldUpdate { get; set; } = false;

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
    
    public void InitilizeAlbumCover(AlbumCover albumCover)
    {
        if(Metadata.AlbumCover is not null)
            throw new InvalidOperationException("The AlbumCover is already initilized.");
        
        Metadata.AlbumCover = albumCover;
    }

    public bool IsSynced()
    {
        string? dbLastModifiedUtcString = DatabaseHelper.Read(
            """
            SELECT last_modified_utc FROM tracks
            WHERE track_id = @track_id
            """,
            reader => reader.GetString(reader.GetOrdinal("last_modified_utc")),
            command => command.Parameters.AddWithValue("track_id", TrackId)
        );

        if(dbLastModifiedUtcString is null)
            return false;

        DateTime dbLastModifiedUtc = DateTime.Parse(
            dbLastModifiedUtcString,
            null,
            DateTimeStyles.RoundtripKind
        );
        DateTime fileLastModifiedUtc = File.GetLastWriteTimeUtc(FilePath);

        Console.WriteLine($"FILE_PATH: {FilePath}");
        Console.WriteLine($"\tDB_LAST_MODIFIED: {dbLastModifiedUtc}");
        Console.WriteLine($"\tFILE_LAST_MODIFIED: {fileLastModifiedUtc}");
        Console.WriteLine($"IS_SYNCED: {dbLastModifiedUtc == fileLastModifiedUtc}");
        Console.Write("\n");

        return dbLastModifiedUtc == fileLastModifiedUtc;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not Track track)
            return false;

        return FilePath == track.FilePath;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FilePath);
    }
}