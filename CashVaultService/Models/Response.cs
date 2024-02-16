using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace CashVaultService.Models
{
    [DataContract]
    public class Response
    {
        [DataMember(Order =0)]public string ResponseCode { get; set; }
        [DataMember(Order =1)]public string ResponseDescription { get; set; }
    }
}