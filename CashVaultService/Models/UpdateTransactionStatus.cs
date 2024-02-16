using System.Runtime.Serialization;

namespace CashVaultService.Models
{
    [DataContract]
    public class UpdateTransactionStatus
    {
        [DataMember]
        public string QTreference { get; set; }
        [DataMember]
        public string Status { get; set; }
    }

    public class UpdateTransactionStatusRes:Response
    {
        public string QTreference { get; set; }
    }
}