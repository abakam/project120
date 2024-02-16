using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace CashVaultService.Models
{
    [DataContract]
    public class CVServiceFault
    {
        private string report;

        public CVServiceFault(string message)
        {
            this.report = message;
        }

        [DataMember]
        public string Message
        {
            get { return this.report; }
            set { this.report = value; }
        }
    }
}