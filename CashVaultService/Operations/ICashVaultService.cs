using CashVaultService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace CashVaultService.Operations
{
    [ServiceContract]
    interface ICashVaultService
    {
        [OperationContract(Name = "UpdateCustomerReversalStatus")]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, UriTemplate = "/customer/reversal")]
        UpdateTransactionStatusRes UpdateCustomerReversalStatus(UpdateTransactionStatus request);

        [OperationContract(Name = "UpdateDisbursementStatus")]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, UriTemplate = "/customer/disbursement")]
        UpdateTransactionStatusRes UpdateDisbursementStatus(UpdateTransactionStatus request);

        [OperationContract(Name = "UpdateMerchantRefundStatus")]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, UriTemplate = "/merchant/refund")]
        UpdateTransactionStatusRes UpdateMerchantRefundStatus(UpdateTransactionStatus request);

        [OperationContract(Name = "AccessFeeDeposit")]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, UriTemplate = "/fee/deposit")]
        Response AccessFeeDeposit(FeeDeposit request);

        [OperationContract(Name = "FundDeposit")]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, UriTemplate = "/fund/deposit")]
        Response FundDeposit(FeeDeposit request);

        [OperationContract(Name = "RevUpdate")]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, UriTemplate = "/revenue")]
        Response RevUpdate(Deposit request);

        [OperationContract(Name = "BalanceAdjust")]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, UriTemplate = "/balance")]
        Response BalanceAdjust(BLAjust request);
    }
}
