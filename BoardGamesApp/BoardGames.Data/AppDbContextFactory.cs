using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BoardGames.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(ResolveConnectionString());

            return new AppDbContext(optionsBuilder.Options);
        }

        private static string ResolveConnectionString()
        {
            string? overrideConnection = Environment.GetEnvironmentVariable("BOOKINGBOARDGAMES_DB_CONNECTION");
            if (!string.IsNullOrWhiteSpace(overrideConnection))
            {
                Console.WriteLine("Using database connection string from environment variable." + overrideConnection);
                return overrideConnection;
            }

            string? webProjectPath = FindWebProjectPath();
            if (webProjectPath != null)
            {
                try
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(webProjectPath)
                        .AddJsonFile("appsettings.json", optional: true)
                        .Build();

                    var defaultConnection = configuration.GetConnectionString("DefaultConnection");
                    if (!string.IsNullOrWhiteSpace(defaultConnection))
                    {
                        Console.WriteLine("Using DefaultConnection from appsettings.json: " + defaultConnection);
                        return defaultConnection;
                    }

                    var remoteConnection = configuration.GetConnectionString("RemoteConnection");
                    if (!string.IsNullOrWhiteSpace(remoteConnection))
                    {
                        Console.WriteLine("Using RemoteConnection from appsettings.json: " + remoteConnection);
                        return remoteConnection;
                    }
                }
                catch
                {
                }
            }

            const string databaseName = "MergedBoardGamesDb";
            string[] candidates =
            {
                $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;",
                $"Server=.\\SQLEXPRESS;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;",
            };

            foreach (string candidate in candidates)
            {
                if (CanConnect(candidate))
                {
                    Console.WriteLine("Using database connection string: " + candidate);
                    return candidate;
                }
            }

            Console.WriteLine("No valid database connection string found. Using default: " + candidates[0]);
            return candidates[0];
        }

        private static bool CanConnect(string connectionString)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Walks up from Directory.GetCurrentDirectory() looking for a sibling
        /// folder named "BookingBoardGamesWeb" that contains appsettings.json.
        /// Works whether the CWD is the bin output folder (runtime) or the
        /// solution/project root (EF CLI migrations).
        /// </summary>
        private static string? FindWebProjectPath()
        {
            const string targetFolder = "BookingBoardGamesWeb";
            const string settingsFile = "appsettings.json";

            DirectoryInfo? directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, targetFolder);
                if (Directory.Exists(candidate) &&
                    File.Exists(Path.Combine(candidate, settingsFile)))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            return null;
        }
    }
}
