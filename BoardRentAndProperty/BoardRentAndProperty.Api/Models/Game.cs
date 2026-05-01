namespace BoardRentAndProperty.Api.Models
{
    public class Game
    {
        public int Id { get; set; }
        public Account? Owner { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int MinimumPlayerNumber { get; set; }
        public int MaximumPlayerNumber { get; set; }
        public string Description { get; set; } = string.Empty;
        public byte[] Image { get; set; } = System.Array.Empty<byte>();
        public bool IsActive { get; set; }

        public Game()
        {
        }

        public Game(int id, Account? gameOwner, string name, decimal price,
                    int minimumPlayerNumber, int maximumPlayerNumber,
                    string description, byte[] image, bool isActive)
        {
            this.Id = id;
            Owner = gameOwner;
            Name = name;
            Price = price;
            MinimumPlayerNumber = minimumPlayerNumber;
            MaximumPlayerNumber = maximumPlayerNumber;
            Description = description;
            Image = image;
            IsActive = isActive;
        }
    }
}
