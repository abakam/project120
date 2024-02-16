using System;

namespace CashVaultService.Models
{
    public class SAReservationOrder : Response
    {
        public string MerchantOrderId { get;  set; }
        public string FundSecureStatus { get;  set; }
        public string Date { get; set; }
        public decimal OrderAmount { get; set; }
    }
}