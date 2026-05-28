// <copyright file="DatabaseConfig.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BoardGames.Desktop
{
    public static class DatabaseConfig
    {
        private const string DatabaseName = "MergedBoardGamesDb";

        public static string ResolveConnectionString()
        {
            string? overrideConnection = Environment.GetEnvironmentVariable("BOOKINGBOARDGAMES_DB_CONNECTION");
            if (!string.IsNullOrWhiteSpace(overrideConnection))
            {
                return overrideConnection;
            }

            string[] candidates =
            {
                $"Server=(localdb)\\Beatrice;Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;",
                $"Server=.\\Beatrice;Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;",
            };

            foreach (string candidate in candidates)
            {
                if (CanConnect(candidate))
                {
                    return candidate;
                }
            }

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
    }
}
