// <copyright file="GameDTO.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BoardGames.Shared.DTO
{
    public class GameDTO : INotifyPropertyChanged
    {
        private int id;
        private decimal price;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <remarks>This event is typically raised by classes that implement the INotifyPropertyChanged
        /// interface to notify subscribers that a property value has changed. Handlers receive the name of the property
        /// that changed in the PropertyChangedEventArgs parameter.</remarks>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the unique identifier for the game.
        /// </summary>
        public int Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(GameId));
                }
            }
        }

        public int GameId
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public UserDTO? Owner { get; set; }

        /// <summary>
        /// Gets or sets the name associated with the object.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the binary image data associated with this instance.
        /// </summary>
        public byte[]? Image { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the price associated with the item.
        /// </summary>
        public decimal Price
        {
            get => price;
            set
            {
                if (price != value)
                {
                    price = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PricePerDay));
                }
            }
        }

        public decimal PricePerDay
        {
            get => price;
            set
            {
                if (price != value)
                {
                    price = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Price));
                }
            }
        }

        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the name of the city associated with the entity.
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum number of players allowed.
        /// </summary>
        public int MaximumPlayerNumber { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of players required to start the game.
        /// </summary>
        public int MinimumPlayerNumber { get; set; }

        /// <summary>
        /// Raises the PropertyChanged event to notify listeners that a property value has changed.
        /// </summary>
        /// <remarks>Use this method to implement the INotifyPropertyChanged interface in data-binding
        /// scenarios. Calling this method with the correct property name ensures that UI elements bound to the property
        /// are updated appropriately.</remarks>
        /// <param name="name">The name of the property that changed. This value is optional and is automatically provided when called from
        /// a property setter.</param>
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
