using CashvaultCore.Core;
using CashvaultCore.Data;
using CashvaultCore.Services;
using CashvaultCore.Utilities;
using CashVaultService.Models;
using System;
using System.Configuration;
using System.Linq;

namespace CashVaultService.Operations
{
    public class CashVaultOperations : ICashVaultService
    {
        private static readonly DataHelper _dataHelper = new DataHelper();
        private static readonly Messaging _msgHelper = new Messaging();
        private static readonly string _errorLogPath = ConfigurationManager.AppSettings["ErrorLoggingPath"];
        private static readonly string _debugTracePath = ConfigurationManager.AppSettings["TraceLoggingPath"];
        private static readonly int fundsTransferCost = Convert.ToInt32(ConfigurationManager.AppSettings["fundsTransferCost"]);
        private static readonly string oauth = ConfigurationManager.AppSettings["Token"];
        

        public UpdateTransactionStatusRes UpdateCustomerReversalStatus(UpdateTransactionStatus request)
        {
            CvdbEntities _db = new CvdbEntities();
            UpdateTransactionStatusRes res = new UpdateTransactionStatusRes();
            try
            {
                var result = _db.CustomerReversals.Where(x => x.QuicktellerRef == request.QTreference);
                if (result != null)
                {
                    foreach (CashvaultCore.Data.CustomerReversal cr in result)
                    {
                        if (request.Status == "90091")
                        {
                            cr.SelfMaintainanceFlag = 0;
                            cr.ReversalStatus = null;
                            if (cr.QuicktellerRef.Length == 15)
                            {
                                int retry = Convert.ToInt32(cr.QuicktellerRef.Last()) + 1;
                                cr.QuicktellerRef = cr.QuicktellerRef.Substring(0, 13) + retry;
                            }
                            else
                            {
                                cr.QuicktellerRef = cr.QuicktellerRef + "001";
                            }
                        }
                        else
                        {
                            cr.ReversalStatus = request.Status;
                        }
                    }
                    _db.SaveChanges();

                    res.QTreference = request.QTreference;
                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                    return res;
                }
                else
                {
                    res.QTreference = request.QTreference;
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Unable to Find Customer Reversal for Specified QTreference";
                    return res;
                }

            }
            catch (Exception ex)
            {
                res.QTreference = request.QTreference;
                Logger.logToFile(ex, _errorLogPath); res.ResponseCode = "01";
                res.ResponseDescription = ex.Message;
                return res;
            }
        }

        public UpdateTransactionStatusRes UpdateDisbursementStatus(UpdateTransactionStatus request)
        {
            CvdbEntities _db = new CvdbEntities();
            UpdateTransactionStatusRes res = new UpdateTransactionStatusRes();
            try
            {
                var result = _db.Disbursments.Where(x => x.DisbursementQuickTellerRef == request.QTreference);
                if (result != null)
                {
                    foreach (Disbursment d in result)
                    {
                        if (request.Status == "90091")
                        {
                            d.SelfServiceMaintenanceFlag = 0;
                            d.DisbursementStatus = null;
                            if (d.DisbursementQuickTellerRef.Length == 15)
                            {
                                int retry = Convert.ToInt32(d.DisbursementQuickTellerRef.Last()) + 1;
                                d.DisbursementQuickTellerRef = d.DisbursementQuickTellerRef.Substring(0, 13) + retry;
                            }
                            else
                            {
                                d.DisbursementQuickTellerRef = d.DisbursementQuickTellerRef + "001";
                            }
                        }
                        else
                        {
                            d.DisbursementStatus = request.Status;
                        }
                    }
                    _db.SaveChanges();

                    res.QTreference = request.QTreference;
                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                    return res;
                }
                else
                {
                    res.QTreference = request.QTreference;
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Unable to Find Disbursement for Specified QTreference";
                    return res;
                }

            }
            catch (Exception ex)
            {
                res.QTreference = request.QTreference;
                Logger.logToFile(ex, _errorLogPath); res.ResponseCode = "01";
                res.ResponseDescription = ex.Message;
                return res;
            }
        }

        public UpdateTransactionStatusRes UpdateMerchantRefundStatus(UpdateTransactionStatus request)
        {
            CvdbEntities _db = new CvdbEntities();
            UpdateTransactionStatusRes res = new UpdateTransactionStatusRes();
            try
            {
                var result = _db.MerchantRefunds.Where(x => x.MerchantQuickTellerRef == request.QTreference);
                if (result != null)
                {
                    foreach (MerchantRefund mr in result)
                    {
                        if (request.Status == "90091")
                        {
                            mr.SelfMaintenanceFlag = 0;
                            mr.MerchantRefundStatus = null;
                            if (mr.MerchantQuickTellerRef.Length == 15)
                            {
                                int retry = Convert.ToInt32(mr.MerchantQuickTellerRef.Last()) + 1;
                                mr.MerchantQuickTellerRef = mr.MerchantQuickTellerRef.Substring(0, 13) + retry;
                            }
                            else
                            {
                                mr.MerchantQuickTellerRef = mr.MerchantQuickTellerRef + "001";
                            }
                        }
                        else
                        {
                            mr.MerchantRefundStatus = request.Status;
                        }
                    }
                    _db.SaveChanges();

                    res.QTreference = request.QTreference;
                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                    return res;
                }
                else
                {
                    res.QTreference = request.QTreference;
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Unable to Find Merchant Refund for Specified QTreference";
                    return res;
                }

            }
            catch (Exception ex)
            {
                res.QTreference = request.QTreference;
                Logger.logToFile(ex, _errorLogPath); res.ResponseCode = "01";
                res.ResponseDescription = ex.Message;
                return res;
            }
        }

        public Response AccessFeeDeposit(FeeDeposit request)
        {
            CvdbEntities _db = new CvdbEntities();
            Response res = new Response();

            try
            {
                if (request.Amount <= 0)
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid Amount: " + request.Amount;
                    return res;
                }

                Merchant merchant = _db.Merchants.FirstOrDefault(m => m.MerchantID == request.MerchantId);
                if (merchant == null)
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid Merchant Details";
                    return res;
                }

                int result = DataHelper.UpdateCashVaultRevenue(request.MerchantId, (request.Amount), "Access Fee Deposit", true);

                if (result > 0)
                {
                    merchant.RenewalDate = DateTime.Now;

                    foreach (MerchantSupport ms in merchant.MerchantSupports)
                    {
                        ms.Restrict_Flag = 0;
                    }

                    _db.SaveChanges();

                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Error. Unable to process Access Fee Deposit";
                }
                return res;
            }
            catch (Exception ex)
            {
                Logger.logToFile(ex, _errorLogPath); res.ResponseCode = "01";
                res.ResponseDescription = ex.Message;
                return res;
            }
        }

        public  Response FundDeposit(FeeDeposit request)
        {
            CvdbEntities _db = new CvdbEntities();
            Response res = new Response();

            try
            {
                if (request.Amount <= 0)
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid Amount: " + request.Amount;
                    return res;
                }

                Merchant merchant = _db.Merchants.FirstOrDefault(m => m.MerchantID == request.MerchantId);
                if (merchant == null)
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid Merchant Details";
                    return res;
                }

                int result = DataHelper.UpdateCashVaultRevenue(request.MerchantId, (request.Amount), "Fund Deposit", true);

                if (result > 0)
                {
                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Error. Unable to process Fund Deposit.";
                }
                return res;
            }
            catch (Exception ex)
            {
                Logger.logToFile(ex, _errorLogPath);
                res.ResponseCode = "01";
                res.ResponseDescription = ex.Message;
                return res;
            }

        }

        public Response RevUpdate(Deposit request)
        {
            CvdbEntities _db = new CvdbEntities();
            Response res = new Response();

            try
            {
                if (request.RevAmount <= 0)
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid revenue amount: " + request.RevAmount;
                    return res;
                }

                decimal updateamount = request.RevAmount - 0;

                Random rnd = new Random();
                int value = rnd.Next(100, 999);

                DateTime now = DateTime.Now;      
                var identifier = "ST" + value + now.Year + now.Month + now.Day + now.Hour ;

                int result = DataHelper.UpdateCashVaultRevenue(identifier, updateamount, request.Narration, true);

                if (result > 0)
                {
                    res.ResponseCode = "00";
                    res.ResponseDescription = "Revenue update successful";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Error. Unable to process revenue update request";
                }
                return res;
            }
            catch (Exception ex)
            {
                Logger.logToFile(ex, _errorLogPath);
                res.ResponseCode = "01";
                res.ResponseDescription = ex.Message;
                return res;
            }

        }

        public Response BalanceAdjust(BLAjust request)
        {
            CvdbEntities _db = new CvdbEntities();
            Response res = new Response();

            try
            {
                if (request.Token != oauth)
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid token: " + request.Token;
                    return res;
                }
                
                if (request.BalanceValue < 0 || request.BalanceValue > DataHelper.getCVRevBalance())
                {
                    res.ResponseCode = "08";
                    res.ResponseDescription = "Balance value to reserve greater than available balance";
                    return res;
                }

                DateTime now = DateTime.Now;
                var BLrequestID = "BL" + now.Millisecond + now.Minute + now.Hour + now.Day + now.Month + now.Year ;

                int result = DataHelper.UpdateCashVaultRevenue(BLrequestID, request.BalanceValue, "Balance Adjustment", false); 

                if (result > 0)
                {
                    res.ResponseCode = "00";
                    res.ResponseDescription = "Successfully updated and new balance: " + DataHelper.getCVRevBalance().ToString();

                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Error: Unable to process balance adjustment request";
                }
                return res;
            }
            catch (Exception ex)
            {
                Logger.logToFile(ex, _errorLogPath);
                res.ResponseCode = "01";
                res.ResponseDescription = ex.Message;
                return res;
            }

        }

    }
}