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

        private static void WriteFromList<T>(List<T> list, string sqlCommand, Action<SqliteCommand, T> configure)
        {
            if (!isDatabaseInitilized)
                throw new Exception("Database must be initialized before this function can be called");
            
            using SqliteConnection connection = GetConnection();
            using SqliteCommand command = new SqliteCommand(sqlCommand, connection);
            
            foreach(T item in list)
            {
                command.Parameters.Clear();
                configure(command, item);
                command.ExecuteNonQuery();
            }
        }

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
            command.Parameters.AddWithValue("album_id", 0); // TODO: Implement
            
            command.Parameters.AddWithValue("title", metadata.Title);
            command.Parameters.AddWithValue("artist", metadata.Artist);
            command.Parameters.AddWithValue("album", metadata.Album);
            command.Parameters.AddWithValue("album_artist", metadata.AlbumArtist);
            command.Parameters.AddWithValue("track_number", metadata.TrackNumber);
            command.Parameters.AddWithValue("disc_number", metadata.DiscNumber);
            command.Parameters.AddWithValue("duration_seconds", metadata.DurationSeconds);
            command.Parameters.AddWithValue("date", metadata.Date.ToString());
            command.Parameters.AddWithValue("genre", metadata.Genre);
        }

        private static Track ReadTrack(SqliteDataReader reader)
        {
            // TODO: Add system which instantiates TrackMetadata from file path if
            // file has been modified since it was last updated to the database
            // TODO: Some of these values may be null. Add null check and default values.
            return new Track(
                reader.GetInt32(reader.GetOrdinal("track_id")),
                reader.GetString(reader.GetOrdinal("file_path")),
                reader.GetInt32(reader.GetOrdinal("album_id")),
                new TrackMetadata(
                    reader.GetString(reader.GetOrdinal("title")),
                    reader.GetString(reader.GetOrdinal("artist")),
                    reader.GetString(reader.GetOrdinal("album")),
                    reader.GetString(reader.GetOrdinal("album_artist")),
                    reader.GetInt16(reader.GetOrdinal("track_number")),
                    reader.GetInt16(reader.GetOrdinal("disc_number")),
                    reader.GetInt16(reader.GetOrdinal("duration_seconds")),
                    DateTime.Parse(reader.GetString(reader.GetOrdinal("date"))),
                    reader.GetString(reader.GetOrdinal("genre"))
                )
            );
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
                        genre TEXT NOT NULL
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
            WriteFromList(
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
                    genre
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
                    @genre
                )
                """,
                WriteTrack
            );

            // This assumes all tracks were added without problems.
            // TODO: Add system to account for failed tracks
            Console.WriteLine($"Added {tracks.Count} tracks to library");
        }

        public static List<Track> GetTracks()
        {
            return ReadToList(
                """
                SELECT * FROM tracks
                ORDER BY track_id ASC
                """,
                ReadTrack
            );
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

        public static List<string> GetAlbumTitles()
        {
            return ReadToList(
                """
                SELECT title FROM albums
                """,
                reader =>
                    reader.GetString(reader.GetOrdinal("title"))
                );            
        }
    }    
}