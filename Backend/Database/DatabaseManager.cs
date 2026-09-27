using System.Globalization;
using Microsoft.Data.Sqlite;
using MusicPlayerApp;

namespace Database
{
    public static class DatabaseManager
    {
        private static string databasePath = "Database/library_database.db";
        private static string connectionString = $"DataSource={databasePath}"; 
        private static bool isDatabaseInitilized = false;

        private static SqliteConnection GetConnection()
        {
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();
            return connection;
        } 

        private static void WriteFromValue(string sqlCommand, Action<SqliteCommand> configure)
        {
             if (!isDatabaseInitilized)
                throw new Exception("Database must be initialized before this function can be called");
            
            using SqliteConnection connection = GetConnection();
            using SqliteCommand command = new SqliteCommand(sqlCommand, connection);

            configure(command);

            command.ExecuteNonQuery();
        }

        private static List<T> WriteFromList<T>(List<T> list, string sqlCommand, Action<SqliteCommand, T> configure)
        {
            if (!isDatabaseInitilized)
                throw new Exception("Database must be initialized before this function can be called");
            
            using SqliteConnection connection = GetConnection();
            using SqliteCommand command = new SqliteCommand(sqlCommand, connection);
            
            List<T> failedItems = new List<T>();

            foreach(T item in list)
            {
                try
                {
                    command.Parameters.Clear();
                    configure(command, item);
                    command.ExecuteNonQuery();
                } 
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    failedItems.Add(item);
                }
            }

            return failedItems;
        }

        private static T? ReadValue<T>(string sqlCommand, Action<SqliteCommand> configure, Func<SqliteDataReader, T> mapFunction)
        {
            if (!isDatabaseInitilized)
                throw new Exception("Database must be initialized before this function can be called");
            
            using SqliteConnection connection = GetConnection();
            using SqliteCommand command = new SqliteCommand(sqlCommand, connection);
            
            configure(command);

            using SqliteDataReader reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return default;
            }

            return mapFunction(reader);
        }

        private static List<T> ReadToList<T>(string sqlCommand, Func<SqliteDataReader, T> mapFunction, Action<SqliteCommand>? configure=null)
        {
            if (!isDatabaseInitilized)
                throw new Exception("Database must be initialized before this function can be called");
            
            using SqliteConnection connection = GetConnection();
            using SqliteCommand command = new SqliteCommand(sqlCommand, connection);
            configure?.Invoke(command);
            
            using SqliteDataReader reader = command.ExecuteReader();

            List<T> list = new List<T>();
            while(reader.Read())
                list.Add(mapFunction(reader));

            return list;
        }

        private static void WriteTrack(SqliteCommand command, Track track)
        {
            TrackMetadata metadata = track.Metadata;

            command.Parameters.AddWithValue("file_path", track.FilePath);
            command.Parameters.AddWithValue("album_id", track.AlbumId);
            
            command.Parameters.AddWithValue("title", metadata.Title);
            command.Parameters.AddWithValue("artist", metadata.Artist);
            command.Parameters.AddWithValue("album", metadata.Album);
            command.Parameters.AddWithValue("album_artist", metadata.AlbumArtist);
            command.Parameters.AddWithValue("track_number", metadata.TrackNumber);
            command.Parameters.AddWithValue("disc_number", metadata.DiscNumber);
            command.Parameters.AddWithValue("duration_seconds", metadata.DurationSeconds);
            command.Parameters.AddWithValue("date", metadata.Date.ToString("O"));
            command.Parameters.AddWithValue("genre", metadata.Genre);
            command.Parameters.AddWithValue("last_modified_utc", metadata.LastModifiedUtc.ToString("O"));
        }

        private static Track ReadTrack(SqliteDataReader reader)
        {
            string filePath = reader.GetString(reader.GetOrdinal("file_path"));            
            DateTime dbLastModifiedUtc = DateTime.Parse(
                reader.GetString(reader.GetOrdinal("last_modified_utc")),
                null,
                DateTimeStyles.RoundtripKind
            );
            DateTime fileLastModifiedUtc = File.GetLastWriteTimeUtc(filePath);

            TrackMetadata trackMetadata;
            bool shouldUpdate = false;

            if (dbLastModifiedUtc != fileLastModifiedUtc)
            {
                trackMetadata = new TrackMetadata(new ATL.Track(filePath));
                shouldUpdate = true;
            }
            else
            {
                trackMetadata = new TrackMetadata(
                    reader.GetString(reader.GetOrdinal("title")),
                    reader.GetString(reader.GetOrdinal("artist")),
                    reader.GetString(reader.GetOrdinal("album")),
                    reader.GetString(reader.GetOrdinal("album_artist")),
                    reader.GetInt32(reader.GetOrdinal("track_number")),
                    reader.GetInt32(reader.GetOrdinal("disc_number")),
                    reader.GetInt16(reader.GetOrdinal("duration_seconds")),
                    DateTime.Parse(reader.GetString(reader.GetOrdinal("date"))),
                    reader.GetString(reader.GetOrdinal("genre")),
                    fileLastModifiedUtc
                );
            }

            Track track = new Track(
                reader.GetInt32(reader.GetOrdinal("track_id")),
                reader.GetString(reader.GetOrdinal("file_path")),
                GetAlbumIdFromAlbum(reader.GetString(reader.GetOrdinal("album")), reader.GetString(reader.GetOrdinal("album_artist"))),
                trackMetadata
            );

            if (shouldUpdate)
                track.ShouldUpdate = true;
            
            return track;
        }

        public static Album ReadAlbum(SqliteDataReader reader)
        {
            Album album = new Album(
                reader.GetInt32(reader.GetOrdinal("album_id")),
                reader.GetString(reader.GetOrdinal("album_title")),
                reader.GetString(reader.GetOrdinal("album_artist"))
            );

            return album;
        }

        public static void InitilizeDatabase()
        {
            if (isDatabaseInitilized)
                return;

            using SqliteConnection connection = GetConnection();
            List<SqliteCommand> commands = new List<SqliteCommand>
            {
                new SqliteCommand(
                    """
                    CREATE TABLE IF NOT EXISTS tracks (
                        track_id INTEGER PRIMARY KEY,
                        file_path TEXT NOT NULL,
                        album_id INTEGER,
                        title TEXT NOT NULL,
                        artist TEXT NOT NULL,
                        album TEXT NOT NULL,
                        album_artist TEXT NOT NULL,
                        track_number INTEGER,
                        disc_number INTEGER,
                        duration_seconds INTEGER,
                        date TEXT NOT NULL,
                        genre TEXT NOT NULL,
                        last_modified_utc TEXT NOT NULL
                    )
                    """, connection),
                new SqliteCommand(
                    """
                    CREATE TABLE IF NOT EXISTS albums (
                        album_id INTEGER PRIMARY KEY,
                        album_title TEXT NOT NULL,
                        album_artist TEXT NOT NULL,
                        track_count INTEGER,
                        disc_count INTEGER,
                        duration_seconds INTEGER
                    )
                    """, connection)
            };

            foreach(SqliteCommand command in commands )
                command.ExecuteNonQuery();

            isDatabaseInitilized = true;
        }

        public static void AddTracks(List<Track> tracks)
        {
            List<Track> failedTracks = WriteFromList(
                tracks,
                """
                INSERT INTO tracks (
                    file_path,
                    album_id,
                    title,
                    artist,
                    album,
                    album_artist,
                    track_number,
                    disc_number,
                    duration_seconds,
                    date,
                    genre,
                    last_modified_utc
                )
                VALUES (
                    @file_path,
                    @album_id,
                    @title,
                    @artist,
                    @album,
                    @album_artist,
                    @track_number,
                    @disc_number,
                    @duration_seconds,
                    @date,
                    @genre,
                    @last_modified_utc
                )
                """,
                WriteTrack
            );

            if (failedTracks.Count > 0)
            {
                Console.WriteLine($"{failedTracks.Count} track(s) failed to import:");
                foreach(Track track in failedTracks)
                {
                    Console.WriteLine($"\t{track.FilePath}");
                }
                Console.Write("\n");
            }

            Console.WriteLine($"Added {tracks.Count - failedTracks.Count} tracks to library");
        }

        public static void UpdateTracks(List<Track> tracks)
        {
            HashSet<int> albumIds = tracks.Select(track => track.AlbumId).ToHashSet();

            List<Track> failedTracks = WriteFromList(
                tracks,
                """
                UPDATE tracks
                SET title = @title,
                    artist = @artist,
                    album = @album,
                    album_artist = @album_artist,
                    track_number = @track_number,
                    disc_number = @disc_number,
                    duration_seconds = @duration_seconds,
                    date = @date,
                    genre = @genre,
                    last_modified_utc = @last_modified_utc,
                    album_id = @album_id
                WHERE track_id = @track_id
                """,
                (command, track) =>
                {
                    TrackMetadata metadata = track.Metadata;
                    int newAlbumId = GetAlbumIdFromAlbum(track.Metadata.Album, track.Metadata.AlbumArtist);

                    if (newAlbumId == 0)
                    {
                        // This album does not exist yet
                        // Create new album
                        newAlbumId = AddAlbum(new Album(track.Metadata.Album, track.Metadata.AlbumArtist));
                    }

                    Console.WriteLine($"New Album Id: {newAlbumId}");

                    command.Parameters.AddWithValue("title", metadata.Title);
                    command.Parameters.AddWithValue("artist", metadata.Artist);
                    command.Parameters.AddWithValue("album", metadata.Album);
                    command.Parameters.AddWithValue("album_artist", metadata.AlbumArtist);
                    command.Parameters.AddWithValue("track_number", metadata.TrackNumber);
                    command.Parameters.AddWithValue("disc_number", metadata.DiscNumber);
                    command.Parameters.AddWithValue("duration_seconds", metadata.DurationSeconds);
                    command.Parameters.AddWithValue("date", metadata.Date.ToString());
                    command.Parameters.AddWithValue("genre", metadata.Genre);
                    command.Parameters.AddWithValue("last_modified_utc", metadata.LastModifiedUtc.ToString("O"));
                    command.Parameters.AddWithValue("album_id", newAlbumId);
                    command.Parameters.AddWithValue("track_id", track.TrackId);  

                    track.AlbumId = newAlbumId;
                }
            );

            if (failedTracks.Count > 0)
            {
                Console.Write("\n");
                Console.WriteLine("Following tracks failed to update:");
                foreach (Track track in failedTracks)
                {
                    Console.WriteLine($"\t{track.FilePath}");
                }
                Console.Write("\n");
            }

            if (albumIds.Count > 0)
            {
                UpdateAlbums(albumIds);
            }
        }

        public static void UpdateAlbums(HashSet<int> albumIds)
        {
            List<Track> tracks = GetTracks(false);
            List<Album> albums = new List<Album>();
            
            foreach (int albumId in albumIds)
            {
                Album? album = ReadValue(
                    """
                    SELECT * FROM albums
                    WHERE album_id = @album_id
                    """,
                    command => command.Parameters.AddWithValue("album_id", albumId),
                    ReadAlbum
                );
                
                if (album != null)
                {
                    albums.Add(album);
                }
                    
            }

            foreach(Album album in albums)
            {
                bool hasTracks = ReadValue(
                    """
                    SELECT EXISTS (
                        SELECT 1
                        FROM tracks
                        WHERE album_id = @album_id
                    )
                    """,
                    command => command.Parameters.AddWithValue("album_id", album.AlbumId),
                    reader => reader.GetBoolean(0)
                );

                if (!hasTracks)
                {
                    // No tracks in this album, we can delete it
                    WriteFromValue(
                        """
                        DELETE FROM albums
                        WHERE album_id = @album_id
                        """,
                        command => command.Parameters.AddWithValue("album_id", album.AlbumId)
                    );
                }
            }
        }

        public static int AddAlbum(Album album)
        {
            // Create the album
            WriteFromValue(
                """
                INSERT INTO albums (album_title, album_artist)
                VALUES (@album_title, @album_artist)
                """,
                command =>
                {
                    command.Parameters.AddWithValue("album_title", album.Title);
                    command.Parameters.AddWithValue("album_artist", album.Artist);
                }
            );

            // Get the albumId
            int albumId = GetAlbumIdFromAlbum(album);

            // Add tracks
            foreach(var pair in album.Discs)
            {
                foreach(Track track in pair.Value)
                {
                    track.AlbumId = albumId;
                }
                AddTracks(pair.Value);
            }

            album.AlbumId = albumId;
            return albumId;
        }

        public static List<Track> GetTracks(bool shouldRescan=true)
        {
            List<Track> tracks = ReadToList(
                """
                SELECT * FROM tracks
                ORDER BY track_id ASC
                """,
                ReadTrack
            );

            if (shouldRescan)
            {
                List<Track> tracksToUpdate = tracks.Where(track => track.ShouldUpdate == true).ToList();
                if (tracksToUpdate.Count > 0)
                    UpdateTracks(tracksToUpdate);
                    Console.WriteLine($"Updated {tracksToUpdate.Count} tracks to the database");
            }
            
            return tracks;
        }

        public static List<Album> GetAlbumsFromTracks(List<Track> tracks)
        {
            List<Album> albums = ReadToList(
                """
                SELECT * FROM albums
                ORDER BY album_id ASC
                """,
                ReadAlbum
            );

            foreach(Album album in albums)
            {
                List<Track> albumTracks = tracks.Where(track => track.AlbumId == album.AlbumId).ToList();
                album.AddTracks(albumTracks);
            }

            return albums;
        }

        public static List<Album> GetAlbums()
        {
            return GetAlbumsFromTracks(GetTracks());
        }

        public static List<string> GetFilePaths()
        {
            return ReadToList(
                """
                SELECT file_path FROM tracks
                """,
                reader => 
                    reader.GetString(reader.GetOrdinal("file_path"))
            );
        }

        public static List<string> GetAlbumKeys()
        {
            return ReadToList(
                """
                SELECT album_title, album_artist FROM albums
                """,
                reader =>
                {
                    string title = reader.GetString(reader.GetOrdinal("album_title")) ?? "__unknown__";
                    string artist = reader.GetString(reader.GetOrdinal("album_artist")) ?? "__unknown__";
                    return Library.SerializeAlbumKey(title, artist);
                });            
        }

        public static int GetAlbumIdFromAlbum(Album album)
        {            
            return ReadValue(
                """
                SELECT album_id FROM albums
                WHERE album_title = @album_title
                AND album_artist = @album_artist
                """,
                command =>
                {
                    command.Parameters.AddWithValue("@album_title", album.Title);
                    command.Parameters.AddWithValue("@album_artist", album.Artist);
                },
                reader =>
                    reader.IsDBNull(reader.GetOrdinal("album_id")) is false ? reader.GetInt32(reader.GetOrdinal("album_id")) : 0
            );
        }
        public static int GetAlbumIdFromAlbum(string albumTitle, string albumArtist)
        {
            return GetAlbumIdFromAlbum(new Album(albumTitle, albumArtist));
        }
    }    
}