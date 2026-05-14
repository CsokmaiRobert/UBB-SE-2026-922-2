namespace BoardRentAndProperty.Contracts.DataTransferObjects
{
    public class LoginDataTransferObject
    {
        public string UsernameOrEmail { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
