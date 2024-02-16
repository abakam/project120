using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace CashVaultService.Models
{
    [DataContract]
    public class GetMerchantClientConfigRes : Response
    {
        [DataMember]
        public string MerchantId { get; set; }
        [DataMember]
        public string SecureCode { get; set; }
        [DataMember]
        public string MerchantClientID { get; set; }
        [DataMember]
        public string MerchantClientSecretKey { get; set; }
    }
}