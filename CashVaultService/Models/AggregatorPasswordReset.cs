namespace CashVaultService.Models
{

    public class AggregatorPasswordReset
    {
        public string AgrregatorId { get; set; }
        public string AgrregatorEmail { get; set; }
    }
    public class AggregatorPasswordResetRes : Response
    {
        public string DefaultPassword { get; set; }
    }
}