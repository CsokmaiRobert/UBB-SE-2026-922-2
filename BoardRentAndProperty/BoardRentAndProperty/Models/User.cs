using BoardRentAndProperty.Mappers;

namespace BoardRentAndProperty.Models
{
    public class User : IEntity
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }

        public User()
        {
        }

        public User(int id)
        {
            this.Id = id;
        }

        public User(int id, string displayName)
        {
            this.Id = id;
            DisplayName = displayName;
        }
    }
}