using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CashvaultCore.Validation
{
    public class Error
    {
        public string ResponseCode{ get; set; }
        public List<string> ResponseMessages { get; set; }
    }
}
