namespace BoardRent.DataTransferObjects
{
    public class ChangePasswordDataTransferObject
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
