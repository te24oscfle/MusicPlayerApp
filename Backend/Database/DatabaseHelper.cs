using System.Globalization;
using Microsoft.Data.Sqlite;
using MusicPlayerApp;

namespace Database
{
    public record FailedItem<T>(
        T Item,
        Exception Exception
    );

    public static class DatabaseHelper
    {
        private static string databasePath = "Database/library_database.db";
        private static string connectionString = $"Data Source={databasePath}";
        private static bool isDatabaseInitilized = false;

        #region Database Helpers

        private static SqliteConnection GetConnection()
        {
            // Create Database directory if it doesn't already exist
            Directory.CreateDirectory("Database");

            // Create and open connection
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();
            return connection;
        }

        public static void InitilizeDatabase()
        {
            // Check if it's already initilized
            if (isDatabaseInitilized)
                return;

            // Open connection
            using SqliteConnection connection = GetConnection();
            List<SqliteCommand> commands = new List<SqliteCommand>
            {
                // Tracks table

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

                // Albums table
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
                    """, connection),
                
                // Playlist Metadata table
                new SqliteCommand(
                    """
                    CREATE TABLE IF NOT EXISTS playlist_metadata (
                        playlist_id INTEGER PRIMARY KEY,
                        title TEXT NOT NULL,
                        description TEXT
                        image_file_path TEXT

                        playlist_type TEXT NOT NULL DEFAULT 'manual'
                            CHECK (playlist_type IN ('manual', 'smart'))
                        
                        smart_rules TEXT
                    )
                    """, connection),
                
                // Playlist Tracks table
                new SqliteCommand(
                    """
                    CREATE TABLE IF NOT EXISTS playlist_tracks (
                        playlist_id INTEGER,
                        position INTERGER,
                        track_id INTEGER,

                        PRIMARY KEY (playlist_id, position),

                        FOREIGN KEY (playlist_id)
                            REFERENCES playlist_metadata(playlist_id)
                            ON DELETE CASCADE,
                        
                        FOREIGN KEY (track_id)
                            REFERENCES tracks(track_id)
                    )
                    """, connection)
            };

            // Execute commands
            foreach(SqliteCommand command in commands )
            {
                command.ExecuteNonQuery();
                command.Dispose();
            }
                

            isDatabaseInitilized = true;
        }

        #endregion
        
        #region General Reader/Writer functions

        public static T? Read<T>(string sqlCommand, Func<SqliteDataReader, T> mapper, Action<SqliteCommand>? configureCommand=null)
        {
            // Check if database is initilized
            if (!isDatabaseInitilized)
                throw new Exception("Database must be initilized before this function can be called.");
            
            // Open connection and create command
            using SqliteConnection connection = GetConnection();
            using SqliteCommand command = new SqliteCommand(sqlCommand, connection);

            // Invoke the configureCommand action only if it was provided
            configureCommand?.Invoke(command);

            // Read data
            using SqliteDataReader reader = command.ExecuteReader();
            
            // No data was found
            if (!reader.Read())
                return default;

            // Data was found
            return mapper(reader);
        }

        public static List<T> ReadToList<T>(string sqlCommand, Func<SqliteDataReader, T> mapper, Action<SqliteCommand>? configureCommand = null)
        {
            // Check if database is initilized
            if (!isDatabaseInitilized)
                throw new Exception("Database must be initilized before this function can be called.");

            // Open connection and create command
            using SqliteConnection connection = GetConnection();
            using SqliteCommand command = new SqliteCommand(sqlCommand, connection);

            // Invoke the configureCommand action only if it was provided
            configureCommand?.Invoke(command);

            // Read data
            using SqliteDataReader reader = command.ExecuteReader();

            List<T> list = new List<T>();
            while (reader.Read())
            {
                list.Add(mapper(reader));
            }

            return list;
        }

        public static bool Write(string sqlCommand, Action<SqliteCommand>? configureCommand = null)
        {
            // Check if database is initilized
            if (!isDatabaseInitilized)
                throw new Exception("Database must be initilized before this function can be called.");

            // Open connection and create command
            using SqliteConnection connection = GetConnection();
            using SqliteCommand command = new SqliteCommand(sqlCommand, connection);

            // Invoke the configureCommand action only if it was provided
            configureCommand?.Invoke(command);

            // Write data
            try
            {
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error when executing following SQL command:\n{command.CommandText}");

                Console.WriteLine($"\nParamaters:");
                foreach(SqliteParameter paramater in command.Parameters)
                {
                    Console.WriteLine($"\t{paramater.ParameterName}: {paramater.Value}");
                }

                Console.WriteLine($"\n{exception}\n");
                return false;
            }
        }

        public static List<FailedItem<T>> WriteFromList<T>(List<T> list, string sqlCommand, Action<SqliteCommand, T> configureCommand)
        {
            // Check if database is initilized
            if (!isDatabaseInitilized)
                throw new Exception("Database must be initilized before this function can be called.");

            // Open connection and create command
            using SqliteConnection connection = GetConnection();
            using SqliteCommand command = new SqliteCommand(sqlCommand, connection);

            List<FailedItem<T>> failedItems = new List<FailedItem<T>>();
            foreach(T item in list)
            {
                try
                {
                    // Configure command
                    command.Parameters.Clear();
                    configureCommand(command, item);

                    // Write data to database
                    command.ExecuteNonQuery();
                } 
                catch (Exception exception)
                {
                    // Something went wrong when writing item to database
                    failedItems.Add(new FailedItem<T>(item, exception));
                }
            }

            return failedItems;
        }

        #endregion

        #region Class-specific read/write functions

        public static Track ReadTrack(SqliteDataReader reader)
        {
            string filePath = reader.GetString(reader.GetOrdinal("file_path"));            
            DateTime dbLastModifiedUtc = DateTime.Parse(
                reader.GetString(reader.GetOrdinal("last_modified_utc")),
                null,
                DateTimeStyles.RoundtripKind
            );
            DateTime fileLastModifiedUtc = File.GetLastWriteTimeUtc(filePath);

            TrackMetadata trackMetadata;
            
            // Should the metadata of the track be updated in the database. False by default, if the file has been modified since it last was last written to database, this will be set to true.
            bool shouldUpdate = false;

            if (dbLastModifiedUtc != fileLastModifiedUtc)
            {
                // Get updated metadata and set flag to update track in database
                trackMetadata = new TrackMetadata(new ATL.Track(filePath));
                shouldUpdate = true;
            }
            else
            {
                // Metadata is already up to date
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
                reader.GetInt32(reader.GetOrdinal("album_id")),
                trackMetadata
            );

            if (shouldUpdate)
                track.ShouldUpdate = true;
            
            return track;
        }

        public static void WriteTrack(SqliteCommand command, Track track)
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
            command.Parameters.AddWithValue("date", metadata.Date.ToString("O")); // "O" means we are using the round-trip date format, which retains precision when serializing and deserializing.
            command.Parameters.AddWithValue("genre", metadata.Genre);
            command.Parameters.AddWithValue("last_modified_utc", metadata.LastModifiedUtc.ToString("O"));
        }
        
        public static void WriteAlbum(SqliteCommand command, Album album)
        {
            command.Parameters.AddWithValue("album_title", album.Title);
            command.Parameters.AddWithValue("album_artist", album.Artist);
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

        #endregion
    }
}