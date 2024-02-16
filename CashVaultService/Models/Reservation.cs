using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace CashVaultService.Models
{
    [DataContract]
    public class ReservationRequest
    {
        [DataMember]
        public string ReservationId { get; set; }
        [DataMember]
        public string OrderNumber { get; set; }
        [DataMember]
        public decimal OrderAmount { get; set; }
        [DataMember]
        public int DeliveryDuration { get; set; }
        [DataMember]
        public decimal DeliveryFee { get; set; }
        [DataMember]
        public int RefundDelivery { get; set; }
        [DataMember]
        public string DispatchName { get; set; }
        [DataMember]
        public string DispatchEmail { get; set; }
        [DataMember]
        public string DispatchMobile { get; set; }
        [DataMember]
        public IEnumerable<ReservationRequestSplit> ReservationRequestSplits { get; set; }
        [DataMember]
        public string ReservationDate { get; set; }
        [DataMember]
        public int Deliverycycles { get; set; }
        [DataMember]
        public int Returns { get; set; }
        [DataMember]
        public string CustomerLink { get; set; }
    }

    [DataContract]
    public class ReservationRequestSplit
    {
        [DataMember]
        public string AccountNo { get; set; }
        [DataMember]
        public string CbnCode { get; set; }
        [DataMember]
        public decimal SplitPercentage { get; set; }
    }

    [DataContract]
    public class ReservationRequestRes : Response
    {
        [DataMember]public string ReservationId { get; set; }
    }  
    
    [DataContract]
    public class SendDeliveryCode
    {
        [DataMember]
        public string ReservationId { get; set; }
        [DataMember]
        public string CustomerCode { get; set; }
        [DataMember]
        public decimal? OrderAmount { get; set; }
    }

    public class SendDeliveryCodeRes : Response
    {

    }

    public class ReservationStatusRes : Response
    {
        public string ReservationId { get; set; }
        public string MerchantName { get; set; }
        public string MerchantOrderNumber { get; set; }
        public decimal MerchantOrderAmount { get; set; }
        public string DeliveryTimeElapse { get; set; }
        public string DurationRemaining { get; set; }
        public string DeliveryCodeStatus { get; set; }
        public string FundSecured { get; set; }
        public string FundSecuredDate { get; set; }
    }

    public class ReservationStatusV2Res : Response
    {
        public IEnumerable<ReservationStatus> ReservationStatusV2 { get; set; }
    }

    public class ReservationStatus
    {
        public string ReservationId { get; set; }
        public decimal OrderAmount { get; set; }
        public string DeliveryDurationRemaining { get; set; }
        public string DeliveryCodeStatus { get; set; }
        public string FundSecured { get; set; }
        public string OrderNo { get; set; }
    }

    public class GetDisburseStatusRes : Response
    {
        public string ReservationId { get; set; }
        public decimal PayoutAmount { get; set; }
        public string OrderTag { get; set; }
        public string PayoutStatus { get; set; }
        public string RetryCount { get; set; }
        public string DisburseDate { get; set; }
    }

    public class ReceiptStatusRes : Response
    {
        public string ReservationId { get; set; }
        public string FulfilmentProgress { get; set; }
        public IEnumerable<Acknowledgement> AcknowledgeTimes { get; set; }
    }

    public class Acknowledgement
    {
        public decimal OrderAmount { get; set; }
        public string Response { get; set; }              
    }

}