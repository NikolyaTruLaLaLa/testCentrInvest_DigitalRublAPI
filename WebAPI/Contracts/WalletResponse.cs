namespace WebAPI.Contracts
{
    public class WalletResponse
    {
        public string Code { get; set; }
        public string Status { get; set; }
        public string? AccountNumber { get; set; }
    }
}
