using CashvaultCore.Data;
using CashvaultCore.Core;
using CashvaultCore.Model;
using CashvaultCore.Services;
using CashvaultCore.Utilities;
using CashVaultService.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;


namespace CashVaultService.Operations
{
    public class ReservationOperations : IReservationService
    {
        private static readonly DataHelper _dataHelper = new DataHelper();
        private static readonly Messaging _msgHelper = new Messaging();
        private static readonly string _errorLogPath = ConfigurationManager.AppSettings["ErrorLoggingPath"];
        private static readonly string _debugTracePath = ConfigurationManager.AppSettings["TraceLoggingPath"];        

        public ReservationRequestRes ReservationRequest(ReservationRequest request)
        {
            CvdbEntities _db = new CvdbEntities();
            var response = new ReservationRequestRes();

            try
            {
                response = ServiceHelper.ValidateReservationRequest(request);
                if (response.ResponseCode == "00")
                {
                    //Insert new Reservation
                    Reservation reservation = new Reservation()
                    {
                        CustomerMobile = request.CustomerLink,
                        ReservationID = request.ReservationId,
                        MerchantOrderAmount = request.OrderAmount,
                        MerchantOrderNumber = request.OrderNumber,

                        //Do some Validation for Delivery Duration and Fee
                        //Set Delivery Duration to 7 days (168 Hours) if Delivery duration is 0
                        DeliveryDuration = request.DeliveryDuration != 0 ? request.DeliveryDuration : 168,

                        //Set Delivery fee to 0 if Delivery Duration is not provided
                        DeliveryFee = request.DeliveryFee != 0 ? request.DeliveryFee : 0,

                        RefundDelivery = request.RefundDelivery,
                        DispatchName = request.DispatchName,
                        DispatchEmail = request.DispatchEmail,
                        DispatchMobileNo = request.DispatchMobile,
                        ReservationDate = Convert.ToDateTime(request.ReservationDate),
                        Deliverycycles = request.Deliverycycles,
                        Returns = request.Returns,                        
                    };

                    //Insert into Fund Source Table
                    FundSource fundSource = new FundSource()
                    {
                        ReservationID = reservation.ReservationID,
                        MerchantOrderAmount = request.OrderAmount,
                        MerchantOrderNumber = request.OrderNumber
                    };

                    _db.FundSources.Add(fundSource);

                    //Insert into ReservationSplit
                    if (request.ReservationRequestSplits != null && request.ReservationRequestSplits.Any())
                    {
                        foreach (
                            var reservationSplit in
                                request.ReservationRequestSplits.Select(requestSplit => new ReservationSplit
                                {
                                    ReservationID = reservation.ReservationID,
                                    AccountNumber = requestSplit.AccountNo,
                                    CbnCode = requestSplit.CbnCode,
                                    SplitPercent = requestSplit.SplitPercentage
                                }))
                        {
                            _db.ReservationSplits.Add(reservationSplit);
                        }
                        reservation.SupportSplits = true;
                    }
                    else
                    {
                        reservation.SupportSplits = false;
                    }
                    //Save all changes to database
                    _db.Reservations.Add(reservation);                    
                    _db.SaveChanges();

                    response.ResponseCode = "00";
                    response.ResponseDescription = "OK";
                    response.ReservationId = reservation.ReservationID;
                }
            }

            catch (Exception ex)
            {
                Logger.logToFile(ex, _errorLogPath);
                response.ResponseCode = "01";
                response.ResponseDescription = ex.Message;
            }
            return response;
        }
        
        public ReservationStatusV2Res GetReservationStatusV2(string DispatchMobile)
        {
            CvdbEntities _db = new CvdbEntities();
            ReservationStatusV2Res result = new ReservationStatusV2Res();

            try
            {              
               var Query = from Reservation in _db.Reservations
                            join FundSource in _db.FundSources on Reservation.ReservationID equals FundSource.ReservationID
                            join Throttle in _db.Throttles on FundSource.ReservationID equals Throttle.ReservationID
                            where (Reservation.ReservationID.StartsWith("A00001") || Reservation.ReservationID.StartsWith("A00002") || Reservation.ReservationID.StartsWith("A00003") || Reservation.ReservationID.StartsWith("A00004"))
                            where Reservation.DispatchMobileNo == DispatchMobile
                            where Throttle.CustomerResponse != "000000"
                            where Throttle.DeliveryTimeElapsed > DateTime.Now

                            select new
                            {
                                ReservationID = Reservation.ReservationID,
                                DeliveryDuration = Reservation.DeliveryDuration,
                                OrderAmount = Reservation.MerchantOrderAmount,
                                FundSecuredDate = FundSource.FundSecuredDate,
                                FundSecuredStatus = FundSource.FundSecured,                               
                                OrderNo = Reservation.MerchantOrderNumber
                            };

                if (Query.Count() == 0)
                {
                    result.ResponseCode = "09";
                    result.ResponseDescription = "There are no active orders for provided mobile number";
                    return result;
                }

                List<ReservationStatus> ReservationStatusV2Collection = new List<ReservationStatus>();
                foreach (var Record in Query)
                {
                    ReservationStatus res = new ReservationStatus();

                    res.FundSecured = Record.FundSecuredStatus;

                    if (Record.FundSecuredDate != null)
                    {
                        TimeSpan timeSpan = ((DateTime)Record.FundSecuredDate).AddHours((Int32)Record.DeliveryDuration) - DateTime.Now;
                        if (timeSpan > TimeSpan.Zero)
                        {
                            res.DeliveryDurationRemaining = String.Format("{0:00}", Math.Truncate(timeSpan.TotalHours)) + ":" + String.Format("{0:00}", timeSpan.Minutes) + ":" + String.Format("{0:00}", timeSpan.Seconds);
                        }
                        else
                        {
                            res.DeliveryDurationRemaining = "00:00:00";
                        }
                    }

                    res.DeliveryCodeStatus = "0";
                    res.ReservationId = Record.ReservationID;
                    res.OrderNo = Record.OrderNo;
                    res.OrderAmount = Record.OrderAmount != null ? Record.OrderAmount.Value : 0;

                    ReservationStatusV2Collection.Add(res);
                }

                result.ReservationStatusV2 = ReservationStatusV2Collection;
                result.ResponseCode = "00";
                result.ResponseDescription = "OK";
                return result;
            }
            catch (Exception ex)
            {
                Logger.logToFile(ex, _errorLogPath);
                result.ResponseCode = "01";
                result.ResponseDescription = ex.Message;
                return result;
            }
        }

        public GetDisburseStatusRes GetDisburseStatus(string ReservationId)
        {
            CvdbEntities _db = new CvdbEntities();
            GetDisburseStatusRes result = new GetDisburseStatusRes();
            
            try
            {
                var merchant = DataHelper.GetMerchantFromReservationId(ReservationId);
                if (merchant == null)
                {
                    result.ResponseCode = "09";
                    result.ResponseDescription = "Invalid merchant";
                    return result;
                }

                Disbursment disbursment = _db.Disbursments.FirstOrDefault(fs => fs.ReservationID == ReservationId);

                if (disbursment != null)
                {
                    result.ReservationId = disbursment.ReservationID;
                    result.PayoutAmount = disbursment.DisbursementAmount != null ? disbursment.DisbursementAmount.Value : 0; 
                    result.RetryCount = disbursment.SelfServiceMaintenanceFlag != null ? disbursment.SelfServiceMaintenanceFlag.Value.ToString() : "0";
                    result.PayoutStatus = disbursment.DisbursementStatus != null ? ServiceHelper.GetStatusString(disbursment.DisbursementStatus) : "Pending";
                    result.OrderTag = disbursment.MerchantOrderNumber != null ? disbursment.MerchantOrderNumber : "Pending";
                    result.DisburseDate = disbursment.DisbursementDate != null ? disbursment.DisbursementDate.Value.ToString("dd-MM-yyyy HH:mm:ss"): "Pending";
                }    
                else
                {
                    result.ResponseCode = "09";
                    result.ResponseDescription = "No disbursement for this order yet";
                    return result;
                }
                                              
                result.ResponseCode = "00";
                result.ResponseDescription = "OK";            
                return result;
            }
            catch (Exception ex)
            {
                Logger.logToFile(ex, _errorLogPath);
                result.ResponseCode = "01";
                result.ResponseDescription = ex.Message;
                return result;
            }
        }

        public ReservationStatusRes GetReservationStatus(string ReservationID)
        {
            return ReservationStatus(ReservationID);
        }

        public ReservationStatusRes ReservationStatus(string ReservationID)
        {
            CvdbEntities _db = new CvdbEntities();
            ReservationStatusRes res = new ReservationStatusRes();

            try
            {
                Reservation Reservation = _db.Reservations.FirstOrDefault(rs => rs.ReservationID == ReservationID);

                if (Reservation == null)
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid order detail";
                    res.ReservationId = ReservationID;
                    return res;
                }
                //Get Merchant by Extracting MerchantID from ReservationID
                Merchant merchant = DataHelper.GetMerchantFromReservationId(ReservationID);

                if (merchant == null)
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "No merchant record found this order";
                    res.ReservationId = ReservationID;
                    return res;
                }

                FundSource fundSource = _db.FundSources.FirstOrDefault(fs => fs.ReservationID == Reservation.ReservationID);

                if (fundSource != null)
                {
                    res.FundSecured = fundSource.FundSecured;
                    res.FundSecuredDate = fundSource.FundSecuredDate != null ? fundSource.FundSecuredDate.Value.ToString("dd-MM-yyyy HH:mm:ss") : null;

                    res.DeliveryTimeElapse = fundSource.FundSecuredDate != null ?
                    (((DateTime)fundSource.FundSecuredDate).AddHours((Int32)Reservation.DeliveryDuration)).ToString("dd-MM-yyyy HH:mm:ss") : null;

                    if (fundSource.FundSecuredDate != null && fundSource.FundSecured == "00")
                    {
                        TimeSpan timeSpan = ((DateTime)fundSource.FundSecuredDate).AddHours((Int32)Reservation.DeliveryDuration) - DateTime.Now;
                        if (timeSpan > TimeSpan.Zero)
                        {
                            res.DurationRemaining = String.Format("{0:00}", Math.Truncate(timeSpan.TotalHours)) + ":" + String.Format("{0:00}", timeSpan.Minutes) + ":" + String.Format("{0:00}", timeSpan.Seconds);
                        }
                        else
                        {
                            res.DurationRemaining = "00:00:00";
                        }
                    }

                    //If DeliveryCode Has Been Provided
                    var CustomerCodeResponseStatus = _db.Throttles.FirstOrDefault(x => x.ReservationID == ReservationID && x.CustomerResponse == "000000");

                    if (CustomerCodeResponseStatus != null)
                    {
                        res.DeliveryCodeStatus = "1";
                        //if Delivery code has been provided Set Duration Remaining to 0
                        res.DurationRemaining = "00:00:00";
                    }
                    else
                    {
                        res.DeliveryCodeStatus = "0";
                    }
                }

                res.ResponseCode = "00";
                res.ResponseDescription = "OK";
                res.ReservationId = Reservation.ReservationID;
                res.MerchantName = merchant.MerchantName;
                res.MerchantOrderNumber = Reservation.MerchantOrderNumber;
                res.MerchantOrderAmount = Reservation.MerchantOrderAmount != null ? Reservation.MerchantOrderAmount.Value : 0;
                return res;

            }
            catch (Exception ex)
            {
                Logger.logToFile(ex, _errorLogPath); res.ResponseCode = "01";
                res.ResponseDescription = ex.Message;
                return res;
            }
        }

        public ReceiptStatusRes GetReceipts(string ReservationID)
        {
            CvdbEntities _db = new CvdbEntities();
            ReceiptStatusRes res = new ReceiptStatusRes();
            List<Acknowledgement> Sublist = new List<Acknowledgement>();

            try
            {
                Reservation Reservation = _db.Reservations.FirstOrDefault(rs => rs.ReservationID == ReservationID);
                
                if (Reservation == null)
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid Reservation Detail";
                    res.ReservationId = ReservationID;
                    return res;
                }

                FundSource fundSource = _db.FundSources.FirstOrDefault(fs => fs.ReservationID == Reservation.ReservationID);

                if (fundSource.FundSecured != "00")
                {
                    res.ResponseCode = "07";
                    res.ResponseDescription = "Order not active";
                    res.ReservationId = ReservationID;
                    return res;
                }

                if (Reservation.Deliverycycles < 2)
                {
                    Throttle throttle = _db.Throttles.FirstOrDefault(fs => fs.ReservationID == ReservationID);
                    if (throttle.CustomerResponse == null)
                    {
                        res.ResponseCode = "06";
                        res.ResponseDescription = "Receipt Pending";
                        res.ReservationId = ReservationID;
                        return res;
                    }
                    else
                    {
                        var SingleDelivery = _db.Throttles.Where(s => s.ReservationID == ReservationID).Select(x => new 
                        {
                            OrderAmount = x.MerchantOrderAmount.Value,
                            Response = x.CustomerResponseTime.Value 
                        });
                        if (SingleDelivery != null)
                        {
                            foreach (var item in SingleDelivery)
                            {
                                Sublist.Add(new Acknowledgement
                                {
                                    OrderAmount = item.OrderAmount,
                                    Response = item.Response.ToString("dd-MM-yyyy HH:mm:ss")
                                });
                            }
                        }
                        res.ResponseCode = "00";
                        res.ResponseDescription = "OK";
                        res.ReservationId = throttle.ReservationID;
                        res.FulfilmentProgress = throttle.CustomerResponse == null ? "0%" : "100%";
                        res.AcknowledgeTimes = Sublist.ToList();
                        return res;
                    }
                }
                else if (Reservation.Deliverycycles > 1)
                {
                    Throttle throttle = _db.Throttles.FirstOrDefault(fs => fs.ReservationID == ReservationID);
                    int ResponseCount = _db.Subthrottles.Where(q => q.ReservationID == ReservationID).Count();
                    decimal? RedeemedValue = _db.Subthrottles.Where(q => q.ReservationID == ReservationID).Sum(q => q.Receiptvalue);

                    var m1 = Convert.ToDecimal(RedeemedValue);
                    var m2 = Convert.ToDecimal(throttle.MerchantOrderAmount);

                    if (ResponseCount < 1)
                    {
                        res.ResponseCode = "06";
                        res.ResponseDescription = "Receipt Pending";
                        res.ReservationId = ReservationID;
                        return res;
                    }
                    else
                    {
                        var MultipleDeliveries = _db.Subthrottles.Where(s => s.ReservationID == ReservationID).Select(x => new 
                        {
                            OrderAmount = x.Receiptvalue.Value,
                            Time = x.Responsetime.Value
                        });

                        if (MultipleDeliveries != null)
                        {
                            foreach (var item in MultipleDeliveries)
                            {
                                Sublist.Add(new Acknowledgement
                                {
                                    OrderAmount = item.OrderAmount,
                                    Response = item.Time.ToString("dd-MM-yyyy HH:mm:ss")
                                });
                            }
                        }
                        res.ResponseCode = "00";
                        res.ResponseDescription = "OK";
                        res.ReservationId = throttle.ReservationID;
                        res.FulfilmentProgress = Convert.ToString(Math.Round(((m1 / m2) * 100), 1)) + "%";
                        res.AcknowledgeTimes = Sublist.ToList();
                        return res;
                    }
                    
                }               
            }
            catch (Exception ex)
            {
                Logger.logToFile(ex, _errorLogPath);
                res.ResponseCode = "01";
                res.ResponseDescription = ex.Message;
                return res;
            }

            return res;
        }
       
        public SendDeliveryCodeRes SendDeliveryCode(SendDeliveryCode request)
        {
            CvdbEntities _db = new CvdbEntities();
            var res = new SendDeliveryCodeRes();
            try
            {
                var Suspect = _db.FundSources.FirstOrDefault(a => a.ReservationID == request.ReservationId);
                var FraudCustomer = _db.Customers.FirstOrDefault(b => b.CustomerID == Suspect.CustomerID);

                if (FraudCustomer.Customer_Flagged == 1)
                {
                    res.ResponseCode = "FD";
                    res.ResponseDescription = "Suspected fraudulent activity for this order " + request.ReservationId;
                    return res;
                }

                var vaultCodeExtraDetails = _db.VaultCodeDetails.FirstOrDefault(d => d.ReservationId == request.ReservationId);
                if (vaultCodeExtraDetails == null)
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "No Notification entry to process request at this time for order: " + request.ReservationId;
                    return res;
                }

                var reservation = vaultCodeExtraDetails.Reservation;
                if (reservation != null)
                {
                    var throttle = _db.Throttles.Where(t => t.ReservationID == request.ReservationId).FirstOrDefault();
                    if (throttle == null)
                    {
                        res.ResponseCode = "09";
                        res.ResponseDescription = "No Throttle entry to process request at this time for order: " + reservation.ReservationID;
                        return res;
                    }

                    if (throttle.CustomerResponse == "000000")
                    {
                        res.ResponseCode = "09";
                        res.ResponseDescription = "Response already received";
                        return res;
                    }

                    if (request.OrderAmount > throttle.MerchantOrderAmount)
                    {
                        res.ResponseCode = "09";
                        res.ResponseDescription = "Invalid receipt amount";
                        return res;
                    }

                    if (throttle.Deliverycycles > 1 && (request.OrderAmount == null || request.OrderAmount <= 0))
                    {
                        res.ResponseCode = "09";
                        res.ResponseDescription = "Provide receipt value for this redemption";
                        return res;
                    }                 

                    int totalRevisions = _db.Subthrottles.Where(q => q.ReservationID == request.ReservationId).Count();
                    decimal? totalReceiptValue = _db.Subthrottles.Where(q => q.ReservationID == request.ReservationId).Sum(q => q.Receiptvalue);
                    
                    if (throttle.Deliverycycles > 1 && (throttle.Deliverycycles == totalRevisions))
                    {
                        res.ResponseCode = "09";
                        res.ResponseDescription = "Delivery cycles complete no more attempts supported";
                        return res;
                    }

                    if (((totalReceiptValue + request.OrderAmount) > throttle.MerchantOrderAmount) && throttle.Deliverycycles > 1)
                    {
                        res.ResponseCode = "09";
                        res.ResponseDescription = "Order amount greater than expected receipt value";
                        return res;
                    }

                    if (DateTime.Now > throttle.DeliveryTimeElapsed)
                    {
                        res.ResponseCode = "09";
                        res.ResponseDescription =
                            $"The reservation expired on {throttle.DeliveryTimeElapsed}. Secured fund has been queued for customer reversal.";
                        return res;
                    }

                    var inVaultCode = request.CustomerCode.ToCharArray();
                    var dbVaultCode = vaultCodeExtraDetails.CustomerVaultCode.ToCharArray();
                    var customer = _db.Customers.Where(q => q.CustomerID == throttle.CustomerID).FirstOrDefault();

                    string vaultCodeCheckResult;

                    var vaultCodeCheck = ServiceHelper.ValidateVaultCode(inVaultCode, dbVaultCode, out vaultCodeCheckResult);

                    if (vaultCodeCheck)
                    {
                        if (throttle.Deliverycycles > 1)
                        {
                            Subthrottle subthrottle = new Subthrottle();
                            subthrottle.Receiptvalue = request.OrderAmount;
                            subthrottle.ReservationID = request.ReservationId;
                            subthrottle.Custresponse = vaultCodeCheckResult;
                            subthrottle.Responsetime = DateTime.Now;
                            _db.Subthrottles.Add(subthrottle);
                            _db.SaveChanges();

                            if (customer != null)
                            {                               
                                customer.Lastredemptndate = subthrottle.Responsetime; 
                                customer.Redemvalue = (customer.Redemvalue ?? 0) + request.OrderAmount;
                                _db.SaveChanges();
                            }

                            totalReceiptValue = _db.Subthrottles.Where(q => q.ReservationID == request.ReservationId).Sum(q => q.Receiptvalue);
                            if (totalReceiptValue == throttle.MerchantOrderAmount)
                            {
                                throttle.CustomerResponse = "000000";
                                throttle.CustomerResponseTime = DateTime.Now;
                                _db.SaveChanges();
                            }
                        }
                        else
                        {
                            if (vaultCodeCheckResult == "000000")
                            {
                                throttle.CustomerResponse = vaultCodeCheckResult;
                                throttle.CustomerResponseTime = DateTime.Now;
                                _db.SaveChanges();
                            }
                            else
                            {
                                res.ResponseCode = "09";
                                res.ResponseDescription = "Invalid Delivery Code";
                                return res;
                            }
                        }                      
                                                
                        if (throttle.CustomerResponse == "000000")
                        {
                            if (customer != null)
                            {
                                customer.Redemption = customer.Redemption > 0 ? customer.Redemption + 1 : 1;                                
                                
                                if (throttle.Deliverycycles < 2)
                                {
                                    customer.Lastredemptndate = throttle.CustomerResponseTime;
                                    customer.Redemvalue = (customer.Redemvalue ?? 0) + throttle.MerchantOrderAmount;
                                }                                                            
                                _db.SaveChanges();
                            }
                        }

                        res.ResponseCode = "00";
                        res.ResponseDescription = "Code validation successful";
                    }
                    else
                    {
                        res.ResponseCode = "09";
                        res.ResponseDescription = $"Invalid Delivery Code: {request.CustomerCode} ";
                    }
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid Reservation ID: " + request.ReservationId;
                }
            }
            catch (Exception ex)
            {
                Logger.logToFile(ex, _errorLogPath);
                res.ResponseCode = "01";
                res.ResponseDescription = ex.Message;
            }
            return res;
        }
    }
}