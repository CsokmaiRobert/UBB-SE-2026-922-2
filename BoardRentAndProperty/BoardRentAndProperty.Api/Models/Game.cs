using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardRentAndProperty.Api.Models
{
    [Table("Games")]
    public class Game
    {
        [Column("game_id")]
        public int Id { get; set; }

        [Column("owner_id")]
        public int OwnerId { get; set; }

        public Account? Owner { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("price")]
        public decimal Price { get; set; }

        [Column("minimum_player_number")]
        public int MinimumPlayerNumber { get; set; }

        [Column("maximum_player_number")]
        public int MaximumPlayerNumber { get; set; }

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("image")]
        public byte[]? Image { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        public Game() { }

        public Game(int id, Account? gameOwner, string name, decimal price,int minimumPlayerNumber, int maximumPlayerNumber,string description, byte[]? image, bool isActive)
        {
            this.Id = id;
            this.Owner = gameOwner;
            this.Name = name;
            this.Price = price;
            this.MinimumPlayerNumber = minimumPlayerNumber;
            this.MaximumPlayerNumber = maximumPlayerNumber;
            this.Description = description;
            this.Image = image;
            this.IsActive = isActive;
        }
    }
}