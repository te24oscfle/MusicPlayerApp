using Microsoft.AspNetCore.Mvc;
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
                catch (Exception)
                {
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
            reader.Read();

            return mapFunction(reader);
        }

        // TODO: Add configure lambda function?
        private static List<T> ReadToList<T>(string sqlCommand, Func<SqliteDataReader, T> mapFunction)
        {
            if (!isDatabaseInitilized)
                throw new Exception("Database must be initialized before this function can be called");
            
            using SqliteConnection connection = GetConnection();
            using SqliteCommand command = new SqliteCommand(sqlCommand, connection);
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
            command.Parameters.AddWithValue("date", metadata.Date.ToString());
            command.Parameters.AddWithValue("genre", metadata.Genre);
            command.Parameters.AddWithValue("last_modified_utc", metadata.LastModifiedUtc);
        }

        private static Track ReadTrack(SqliteDataReader reader)
        {
            string filePath = reader.GetString(reader.GetOrdinal("file_path"));            
            DateTime dbLastModifiedUtc = DateTime.Parse(reader.GetString(reader.GetOrdinal("last_modified_utc")));
            DateTime fileLastModifiedUtc = File.GetLastWriteTimeUtc(filePath);

            TrackMetadata trackMetadata;
            bool shouldUpdate = false;
            if (dbLastModifiedUtc == fileLastModifiedUtc)
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
                    DateTime.Parse(reader.GetString(reader.GetOrdinal("last_modified_utc")))
                );
            } 
            else
            {
                trackMetadata = new TrackMetadata(new ATL.Track(filePath));
                shouldUpdate = true;
            }

            Track track = new Track(
                reader.GetInt32(reader.GetOrdinal("track_id")),
                reader.GetString(reader.GetOrdinal("file_path")),
                reader.GetInt32(reader.GetOrdinal("album_id")),
                trackMetadata
            );

            if (shouldUpdate)
                track.ShouldUpdate = true;
            
            return track;
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

        public static void UpdateTrack(Track track)
        {
            WriteFromValue(
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
                    last_modified_utc = @last_modified_utc
                WHERE track_id = @track_id
                """,
                command =>
                {
                    TrackMetadata metadata = track.Metadata;

                    command.Parameters.AddWithValue("title", metadata.Title);
                    command.Parameters.AddWithValue("artist", metadata.Artist);
                    command.Parameters.AddWithValue("album", metadata.Album);
                    command.Parameters.AddWithValue("album_artist", metadata.AlbumArtist);
                    command.Parameters.AddWithValue("track_number", metadata.TrackNumber);
                    command.Parameters.AddWithValue("disc_number", metadata.DiscNumber);
                    command.Parameters.AddWithValue("duration_seconds", metadata.DurationSeconds);
                    command.Parameters.AddWithValue("date", metadata.Date.ToString());
                    command.Parameters.AddWithValue("genre", metadata.Genre);
                    command.Parameters.AddWithValue("last_modified_utc", metadata.LastModifiedUtc.ToString());
                    command.Parameters.AddWithValue("track_id", track.TrackId);
                }
            );
        }

        public static void AddAlbum(Album album)
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

            // TODO: Get the albumId
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
        }

        public static List<Track> GetTracks()
        {
            List<Track> tracks = ReadToList(
                """
                SELECT * FROM tracks
                ORDER BY track_id ASC
                """,
                ReadTrack
            );

            int updatedTracks = 0;
            foreach(Track track in tracks)
            {
                if (track.ShouldUpdate)
                {
                    UpdateTrack(track);
                    track.ShouldUpdate = false;
                    updatedTracks++;
                }
            }

            if (updatedTracks > 0)
                Console.WriteLine($"Updated {updatedTracks} tracks to the database");

            return tracks;
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

        // TODO: Change thid to GetAlbumKeys
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
                    reader.GetInt32(reader.GetOrdinal("album_id"))
            );
        }
        public static int GetAlbumIdFromAlbum(string albumTitle, string albumArtist)
        {
            return GetAlbumIdFromAlbum(new Album(albumTitle, albumArtist));
        }
    }    
}