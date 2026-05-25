// <copyright file="GameImageMapper.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Api.Mappers
{
    public static class GameImageMapper
    {
        private static readonly Dictionary<string, string> ImageUrls = new()
        {
            { "Catan", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Monopoly", "https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Monopoly.jpg/1920px-Monopoly.jpg" },
            { "Chess", "https://upload.wikimedia.org/wikipedia/commons/thumb/6/6f/ChessSet.jpg/500px-ChessSet.jpg" },
            { "Activity", "https://cf.geekdo-images.com/91COAIDHz_rkVyqC4kezvQ__opengraph/img/8m4xxs0n4HTfeLlaLyD1uhVzxvg=/0x0:1054x553/fit-in/1200x630/filters:strip_icc()/pic6911655.jpg" },
            { "Carcassonne", "https://s13emagst.akamaized.net/products/17087/17086837/images/res_69cecb93f068b6ded94c2efc380b6303.jpg" },
            { "Terraforming Mars", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Ticket to Ride", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Pandemic", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "7 Wonders", "https://upload.wikimedia.org/wikipedia/en/0/0b/7_Wonders_-_New_Edition_boxart.png" },
            { "Azul", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Dixit", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Splendor", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Codenames", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Risk", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Dominion", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Love Letter", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Scythe", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Wingspan", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Gloomhaven", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Brass Birmingham", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Root", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Terraforming Mars: Ares", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Ark Nova", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Everdell", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "The Crew", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Hanabi", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Agricola", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Patchwork", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Carcassonne: Expansion", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Uno", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Exploding Kittens", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
            { "Bang!", "https://upload.wikimedia.org/wikipedia/en/a/a3/Catan-2015-boxart.jpg" },
        };

        private const string FallbackUrl = "https://placehold.co/220x180?text=No+Image";

        public static string GetImageUrl(string gameName)
        {
            return ImageUrls.TryGetValue(gameName, out var url) ? url : FallbackUrl;
        }
    }
}
