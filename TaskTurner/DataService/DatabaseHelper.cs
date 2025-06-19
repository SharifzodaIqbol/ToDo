using System.Data.SQLite;

namespace TaskTurner.DataService
{
    public static class DatabaseHelper
    {
        public static void InitializeDatabase()
        {
            using var connection = new SQLiteConnection("Data Source=users.db");
            connection.Open();

            string createUsersTable = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL
                )";

            string createTasksTable = @"
                CREATE TABLE IF NOT EXISTS Tasks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    IsCompleted INTEGER DEFAULT 0,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
                )";

            using (var command = new SQLiteCommand(createUsersTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SQLiteCommand(createTasksTable, connection))
            {
                command.ExecuteNonQuery();
            }

            AddMissingColumns(connection);
        }

        private static void AddMissingColumns(SQLiteConnection connection)
        {
            string[] columnsToCheck = { "IsCompleted" };

            foreach (var column in columnsToCheck)
            {
                var checkCmd = new SQLiteCommand($"PRAGMA table_info(Tasks);", connection);
                var reader = checkCmd.ExecuteReader();
                bool columnExists = false;

                while (reader.Read())
                {
                    if (reader.GetString(1) == column)
                    {
                        columnExists = true;
                        break;
                    }
                }

                if (!columnExists)
                {
                    var addCmd = new SQLiteCommand(
                        $"ALTER TABLE Tasks ADD COLUMN {column} INTEGER DEFAULT 0",
                        connection);
                    addCmd.ExecuteNonQuery();
                }
            }
        }
    }
}