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
    interface IReservationService
    {
        [OperationContract(Name = "ReservationRequest")]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, UriTemplate = "")]
        ReservationRequestRes ReservationRequest(ReservationRequest request);

        [OperationContract(Name = "GetReservationStatusV2")]
        [WebGet(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, UriTemplate = "/v2?DispatchMobile={DispatchMobile}")]
        ReservationStatusV2Res GetReservationStatusV2(string DispatchMobile);

        [OperationContract(Name = "GetDisbursementStatus")]
        [WebGet(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, UriTemplate = "/disbursement?ReservationId={ReservationId}")]
        GetDisburseStatusRes GetDisburseStatus(string ReservationId);

        [OperationContract(Name = "GetReservationStatus")]
        [WebGet(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, UriTemplate = "?ReservationId={ReservationId}")]
        ReservationStatusRes GetReservationStatus(string ReservationID);

        [OperationContract(Name = "GetReceipts")]
        [WebGet(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, UriTemplate = "/receipt?ReservationId={ReservationId}")]
        ReceiptStatusRes GetReceipts(string ReservationID);

        [OperationContract(Name = "SendDeliveryCode")]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, UriTemplate = "/delivery")]
        SendDeliveryCodeRes SendDeliveryCode(SendDeliveryCode request);
    }
}
