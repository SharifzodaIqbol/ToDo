using System.Data.SQLite;

namespace TaskTurner.DataService
{
    public static class DatabaseHelper
    {
        public static void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection("Data Source=users.db"))
            {
                connection.Open();

                // Create Users table
                string createUsersTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        Password TEXT NOT NULL
                    );";
                using (var command = new SQLiteCommand(createUsersTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create Tasks table
                string createTasksTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        Title TEXT NOT NULL,
                        Description TEXT,
                        IsCompleted INTEGER NOT NULL DEFAULT 0,
                        TaskImportance TEXT,
                        ProjectId INTEGER,
                        CategoryId INTEGER,
                        TagId INTEGER, -- Add TagId column here
                        FOREIGN KEY (UserId) REFERENCES Users(Id),
                        FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
                        FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
                        FOREIGN KEY (TagId) REFERENCES Tags(Id) -- Add foreign key for Tags
                    );";
                using (var command = new SQLiteCommand(createTasksTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create Projects table
                string createProjectsTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Projects (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        UserId INTEGER NOT NULL,
                        FOREIGN KEY (UserId) REFERENCES Users(Id)
                    );";
                using (var command = new SQLiteCommand(createProjectsTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create Categories table
                string createCategoriesTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Categories (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        UserId INTEGER NOT NULL,
                        FOREIGN KEY (UserId) REFERENCES Users(Id)
                    );";
                using (var command = new SQLiteCommand(createCategoriesTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create Tags table <<<<<<<<<<<<< ADD THIS TABLE CREATION
                string createTagsTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Tags (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        UserId INTEGER NOT NULL,
                        FOREIGN KEY (UserId) REFERENCES Users(Id)
                    );";
                using (var command = new SQLiteCommand(createTagsTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}