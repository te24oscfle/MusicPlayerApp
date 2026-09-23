using Microsoft.Data.Sqlite;
using MusicPlayerApp;

namespace Database
{
    public static class DatabaseManager
    {
        private static string connectionString = "DataSource=Database/library_database.db"; 
        private static bool isDatabaseInitilized = false;

        private static SqliteConnection GetConnection()
        {
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();
            return connection;
        } 

        public static void InitilizeDatabase()
        {
            if (isDatabaseInitilized)
                return;
            
            List<SqliteCommand> commands = new List<SqliteCommand>();

            using SqliteConnection connection = GetConnection();

            SqliteCommand createTrackTableCommand = new SqliteCommand(
                """
                CREATE TABLE IF NOT EXISTS tracks (
                    track_id INTEGER PRIMARY KEY,
                    file_path TEXT NOT NULL,
                    release_id INTEGER
                )
                """, connection);
            commands.Add(createTrackTableCommand);

            foreach(SqliteCommand command in commands )
            {
                command.ExecuteNonQuery();
            }

            isDatabaseInitilized = true;
        }
    }    
}