using System.Text.Json.Serialization;

namespace GUI_BRAP.Models
{
    public class AdminAccountViewModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("role")]
        public RoleViewModel Role { get; set; }

        [JsonPropertyName("isSuspended")]
        public bool IsSuspended { get; set; }

        [JsonPropertyName("isLockedOut")]
        public bool IsLockedOut { get; set; }
    }
}