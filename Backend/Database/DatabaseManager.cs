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

        private static void WriteFromList<T>(List<T> list,string sqlCommand, Action<SqliteCommand, T> configure)
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
                    release_id INTEGER
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
                INSERT INTO tracks (file_path)
                VALUES (@file_path)
                """,
                (command, track) =>
                    command.Parameters.AddWithValue("file_path", track.FilePath)
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
                reader => new Track(
                    reader.GetInt32(reader.GetOrdinal("track_id")),
                    reader.GetString(reader.GetOrdinal("file_path"))
                )
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
    }    
}