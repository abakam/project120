using CashvaultCore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace CashVaultService
{
    public static class SecurityHelper
    {
        private static readonly CvdbEntities _db = new CvdbEntities();

        private static bool CheckAggregatorMerchantLink(string aggregatorID, string merchantID)
        {
            bool result = false;

            var amList = _db.AggregatorMerchants.Where(am => am.AggregatorID == aggregatorID);
            if (amList.Count() > 0)
            {
                var merchList = amList.Select(m => m.MerchantID);
                result = merchList.Contains(merchantID);
            }

            return result;
        }

        internal static bool CheckAccess(string methodName, string requestedID)
        {
            bool result = false;
            int UserGroupID = 0;
            string merchID = "";
            string aggrID = "";
            bool isMerch = false;
            string clientID = "";
            string clientKey = "";

            if (OperationContext.Current.Extensions.Count > 0)
            {
                var clientHeaders = (OperationContext.Current.Extensions.First() as WebOperationContext).IncomingRequest.Headers;
                if (clientHeaders.AllKeys.Contains("ClientID") || clientHeaders.AllKeys.Contains("clientid"))
                {
                    clientID = clientHeaders["ClientID"];
                }
                else
                {
                    return false;
                }

                if (clientHeaders.AllKeys.Contains("ClientSecretKey") || clientHeaders.AllKeys.Contains("clientsecretkey"))
                {
                    clientKey = clientHeaders["ClientSecretKey"];
                }
                else
                {
                    return false;
                }
            }
            else if (OperationContext.Current.IncomingMessageHeaders != null)
            {

                int clientIDHeaderIndex = OperationContext.Current.IncomingMessageHeaders.FindHeader("ClientID", "http://schemas.xmlsoap.org/soap/envelope/");

                if (clientIDHeaderIndex >= 0)
                {
                    XmlReader r = OperationContext.Current.IncomingMessageHeaders.GetReaderAtHeader(clientIDHeaderIndex).ReadSubtree();
                    clientID = XElement.Load(r).Value;
                }
                else
                {
                    return false;
                }

                int clientKeyIDHeaderIndex = OperationContext.Current.IncomingMessageHeaders.FindHeader("ClientSecretKey", "http://schemas.xmlsoap.org/soap/envelope/");
                if (clientKeyIDHeaderIndex >= 0)
                {
                    XmlReader r = OperationContext.Current.IncomingMessageHeaders.GetReaderAtHeader(clientKeyIDHeaderIndex).ReadSubtree();
                    clientKey = XElement.Load(r).Value;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            if (clientID.StartsWith("1")) //Merchant
            {
                var merch = _db.MerchantSecures.Where(ms => ms.MerchantClientID == clientID && ms.MerchantClientSecretKey == clientKey);
                if (merch.Count() == 1)
                {
                    merchID = merch.First().MerchantID;
                    if (merchID == "A00000")
                    {
                        return true;
                    }
                    UserGroupID = 1;
                    isMerch = true;
                }
            }
            else if (clientID.StartsWith("5")) //Aggregator
            {
                var aggr = _db.AggregatorSecures.Where(ms => ms.AggregatorClientID == clientID && ms.AggregatorClientSecretKey == clientKey);
                if (aggr.Count() == 1)
                {
                    aggrID = aggr.First().AggregatorID;
                    var userGroup = _db.UserGroupMemberships.Where(m => m.UserID == aggrID);

                    if (userGroup.Count() == 1)
                    {
                        UserGroupID = userGroup.First().UserGroupID;
                    }
                }
            }
            else
            {
                return false;
            }

            var perm = _db.UserGroupPermissions.Where(p => p.UserGroupID == UserGroupID && p.MethodName == methodName);

            result = perm.Count() > 0;

            if (requestedID != null)
            {
                bool checkRequestedID = false;
                if (requestedID.Length == 6)
                {
                    if (isMerch)
                    {
                        checkRequestedID = requestedID == merchID;
                    }
                    else
                    {
                        checkRequestedID = CheckAggregatorMerchantLink(aggrID, requestedID);
                    }
                }
                else if (requestedID.Length == 7)
                {
                    checkRequestedID = requestedID == aggrID;
                }
                else if (requestedID.Length > 7)
                {
                    if (isMerch)
                    {
                        checkRequestedID = requestedID.Substring(0, 6) == merchID;
                    }
                    else
                    {
                        checkRequestedID = CheckAggregatorMerchantLink(aggrID, requestedID.Substring(0, 6));
                    }
                }

                result = result & checkRequestedID;
            }

            return result;
        }
    }
}