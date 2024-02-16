using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Web;

namespace CashVaultService.Security
{
    public class ClientIDInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            request.
            string clientID = request.Headers.GetHeader<string>("ClientID", "http://yournamespace");

            if (!IsValidClientID(clientID))
            {
                throw new SecurityException("Invalid client ID");
            }

            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
           // throw new NotImplementedException();
        }
    }
}