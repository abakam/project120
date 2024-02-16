using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace CashVaultService.Models.Merchant
{
    [DataContract]
    public class CreateMerchant
    {
        [DataMember]
        public string MerchantName { get; set; }
        [DataMember]
        public decimal MerchantTrxnFee { get; set; }
        [DataMember]
        public decimal MerchantMinOrderAmount { get; set; }
        [DataMember]
        public decimal MerchantDisbursementAmount { get; set; }
        [DataMember]
        public int MerchantSplits { get; set; }
        [DataMember]
        public string MerchantLastname { get; set; }
        [DataMember]
        public string MerchantFirstName { get; set; }
        [DataMember]
        public string MerchantUserEmail { get; set; }
        [DataMember]
        public string MerchantUserMobile { get; set; }
    }

    [DataContract]
    public class CreateBusinessOwner
    {
        [DataMember]
        public string MerchantLastname { get; set; }
        [DataMember]
        public string MerchantFirstName { get; set; }
        [DataMember]    
        public string MerchantName { get; set; }
        [DataMember]
        public string MerchantUserEmail { get; set; }
        [DataMember]
        public string DefaultMerchantUserPwd { get; set; }
        [DataMember]
        public string MerchantUserMobile { get; set; }
    }
    [DataContract]
    public class CreateMerchantRes : Response
    {
        [DataMember]
        public string MerchantID { get; set; }
    }
    [DataContract]
    public class ChangeMerchantUserPassword
    {
        [DataMember]
        public string MerchantID { get; set; }
        [DataMember]
        public string MerchantUserEmail { get; set; }
        [DataMember]
        public string DefaultMerchantUserPwd { get; set; }
        [DataMember]
        public string NewPwd { get; set; }
        [DataMember]
        public string ConfirmNewPwd { get; set; }
    }
    [DataContract]
    public class ChangeMerchantUserPasswordRes : Response
    {
        [DataMember]
        public int MerchantSupportID { get; set; }
        [DataMember]
        public string MerchantUserEmail { get; set; }
    }
    [DataContract]
    public class CreateMerchantUser
    {
        [DataMember]
        public string MerchantID { get; set; }
        [DataMember]
        public int MerchantUserDispatch { get; set; }
        [DataMember]
        public int MerchantUserReport { get; set; }
        [DataMember]
        public string MerchantLastname { get; set; }
        [DataMember]
        public string MerchanFirstName { get; set; }
        [DataMember]
        public string MerchantUserEmail { get; set; }
        [DataMember]
        public string MerchantUserPwd { get; set; }
        [DataMember]
        public string MerchantUserMobile { get; set; }
    }
    [DataContract]
    public class CreateMerchantUserRes : Response
    {
        [DataMember]
        public int MerchantSupportID { get; set; }
    }
    [DataContract]
    public class DeleteMerchantUser
    {
        [DataMember]
        public string Merchantid { get; set; }
        [DataMember]
        public string Merchantemailaddress { get; set; }
    }
    [DataContract]
    public class MerchantUserLogin
    {
        [DataMember]
        public string Useremailaddress { get; set; }
        [DataMember]
        public string Password { get; set; }
    }
    [DataContract]
    public class MerchantUserLoginRes : Response
    {
        [DataMember]
        public string Merchantid { get; set; }
        [DataMember]
        public int MerchantSupportid { get; set; }
        [DataMember]
        public string Merchantname { get; set; }
        [DataMember]
        public string Firstname { get; set; }
        [DataMember]
        public string Lastname { get; set; }
        [DataMember]
        public string Usrrole { get; set; }
        [DataMember]
        public int? Restrictstatus { get; set; }
    }
    [DataContract]
    public class EditMerchantUser
    {
        [DataMember]
        public string Merchantid { get; set; }
        [DataMember]
        public string Merchantemailaddress { get; set; }
        [DataMember]
        public string Updatefirstname { get; set; }
        [DataMember]
        public string Updatelastname { get; set; }
        [DataMember]
        public string Updateemailaddress { get; set; }
        [DataMember]
        public string Updatemobilenumber { get; set; }
    }
    [DataContract]
    public class GenerateMerchantClientKeyRes : Response
    {
        [DataMember]
        public string ClientKey { get; set; }
    }

    [DataContract]
    public class MerchantReturnsRequest
    {
        [DataMember]
        public string Reservationid { get; set; }
        [DataMember]
        public decimal Custrefundamount { get; set; }
    }

    [DataContract]
    public class MerchantReturnsRes : Response
    {
        public string ReservationId { get; set; }
    }

    [DataContract]
    public class MigrateRefundRequest
    {
        [DataMember]
        public string Reservationid { get; set; }
        [DataMember]
        public string Tag { get; set; }
    }

    [DataContract]
    public class MigrateRefundRes : Response
    {
        public string ReservationId { get; set; }
    }
}

[DataContract]
public class MerchantItem
{
    [DataMember]
    public string MerchantID { get; set; }
    [DataMember]
    public string MerchantName { get; set; }
}