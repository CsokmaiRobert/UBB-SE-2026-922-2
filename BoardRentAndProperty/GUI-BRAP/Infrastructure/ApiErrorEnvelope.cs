namespace GUI_BRAP.Infrastructure
{
    public sealed class ApiErrorEnvelope
    {
        public string? Code { get; set; }
        public string? Error { get; set; }
        public int Status { get; set; }
    }
}
