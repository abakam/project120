using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashvaultCore.Model
{
    public class SMSResponse
    {
        public List<message> messages { get; set; }
    }

    //{"messages":[
    //{"to":"2348137674206",
    //"status":{"groupId":5,"groupName":"REJECTED","id":12,"name":"REJECTED_NOT_ENOUGH_CREDITS","description":"Not enough credits"},
    //"smsCount":1,"messageId":"fddbde65-8b47-4469-a5f4-24b8860ce00b"}]}

    public class message
    {
        public string to { get; set; }
        public status status { get; set; }
        public int smsCount { get; set; }
        public string messageId { get; set; }
    }

    public class status {
        public int groupId { get; set; }
        public string groupName { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class BulksmsNigeriaResponse
    {
        public BulksmsNigeriaResponseData data { get; set; }
        public List<BulksmsNigeriaResponseError> error { get; set; }
    }

    public class BulksmsNigeriaResponseData
    {
        public string status { get; set; }
        public string message { get; set; }
    }

    public class BulksmsNigeriaResponseError
    {
        public List<string> message { get; set; }
    }
}
