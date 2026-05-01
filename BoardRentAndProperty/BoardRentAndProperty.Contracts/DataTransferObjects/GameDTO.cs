namespace BoardRentAndProperty.Contracts.DataTransferObjects
{
    public class GameDTO
    {
        public int Id { get; set; }
        public UserDTO Owner { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int MinimumPlayerNumber { get; set; }
        public int MaximumPlayerNumber { get; set; }
        public string Description { get; set; }
        public byte[] Image { get; set; }
        public bool IsActive { get; set; }

        public GameDTO()
        {
        }
    }
}
