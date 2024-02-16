using CashvaultCore.Data;
using CashvaultCore.Services;
using CashvaultCore.Utilities;
using CashVaultService.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;

namespace CashVaultService.Operations
{
    public static class MiscellaneousOperations
    {
        private static readonly DataHelper _dataHelper = new DataHelper();
        private static readonly Messaging _msgHelper = new Messaging();
        private static readonly string _errorLogPath = ConfigurationManager.AppSettings["ErrorLoggingPath"];
        private static readonly string _debugTracePath = ConfigurationManager.AppSettings["TraceLoggingPath"];
        private static readonly int _fundsTransferCost = Convert.ToInt32(ConfigurationManager.AppSettings["_fundsTransferCost"]);

        //o	Return: Return all columns except Cust_bvn and vault password
        public static GetCustomerById GetCustomerById(int customerId)
        {
            CvdbEntities _db = new CvdbEntities();
            GetCustomerById res = new GetCustomerById();
            try
            {
                var result = _db.Customers.FirstOrDefault(x => x.CustomerID == customerId);
                if (result != null)
                {
                    res.CustomerEmail = result.CustomerEmail;
                    res.CustomerFirstName = result.CustomerFirstName;
                    res.CustomerLastName = result.CustomerLastName;
                    res.CustomerMiddleName = result.CustomerMiddleName;
                    res.CustomerMobile = result.CustomerMobile;
                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Failed";
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

        //o	Return: Return order amount and  where secure_fund_status is 00
        public static GetCustomerOrders GetCustomerOrders(int customerId, DateTime fundSecuredDate)
        {
            CvdbEntities _db = new CvdbEntities();
            GetCustomerOrders res = new GetCustomerOrders();
            try
            {
                var result = _db.FundSources.Where(x => x.FundSecured == "00"
                && x.CustomerID == customerId
                && x.FundSecuredDate.Value == fundSecuredDate);
                if (result != null)
                {
                    var customerOrders = result.Select(x => new CustomerOrder
                    {
                        orderAmount = x.MerchantOrderAmount.Value,
                        reservationId = x.ReservationID
                    });
                    res.CustomerOrders = customerOrders.ToList();
                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Failed";
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

        //o	Return: Return Reversal amount, Reversal status and date where Reversal_status is “00”
        public static GetCustomerReversals GetCustomerReversals(int customerId, DateTime reversalDate)
        {
            CvdbEntities _db = new CvdbEntities();
            GetCustomerReversals res = new GetCustomerReversals();
            try
            {
                var result = _db.CustomerReversals.Where(x => x.ReversalStatus == "00"
                    && x.CustomerID == customerId
                    && x.ReversalDate.Value == reversalDate);
                if (result != null)
                {
                    var customerReversals = result.Select(x => new Models.CustomerReversal
                    {
                        ReversalStatus = x.ReversalStatus,
                        ReversalAmount = x.ReversalAmount.Value,
                        ReversalDate = x.ReversalDate.Value
                    });
                    res.CustomerReversals = customerReversals.ToList();
                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Failed";
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

        //Query against Throttle_tbl Join Fund Source
        //Returns reservation_id, order amount, secured_fund_date, secured_fund_status, 
        //Delivery_Duration_remaining and customer_vault_code status( 1 "true" or  0 "false")
        //Selection Criterion: Merchant_id matches prefix of reservation_id within defined duration
        public static MerchantOrders GetMerchantOrders(string merchantID, DateTime startDate, DateTime endDate)
        {
            CvdbEntities _db = new CvdbEntities();
            MerchantOrders res = new MerchantOrders();
            try
            {
                string prefix = merchantID == "A00000" ? "A" : merchantID;
                var mOrders = (from Throttle in _db.Throttles
                                               join fSource in _db.FundSources on Throttle.ReservationID equals fSource.ReservationID
                                               where Throttle.ReservationID.StartsWith(prefix)
                                               where fSource.FundSecuredDate >= startDate
                                               where fSource.FundSecuredDate <= endDate

                                               select new
                                               {
                                                   ReservationId = Throttle.ReservationID,
                                                   DeliveryTimeElapse = Throttle.DeliveryTimeElapsed.Value,
                                                   OrderAmount = Throttle.MerchantOrderAmount.Value,
                                                   SecuredFundDate = fSource.FundSecuredDate.Value,
                                                   SecuredFundStatus = fSource.FundSecured,
                                                   VaultCodeStatus = Throttle.CustomerResponse == null ? "0" : "1",
                                                   OrderNumber = Throttle.MerchantOrderNumber
                                               });

                if (mOrders != null)
                {
                    res._MerchantOrders = new List<MerchantOrder>();

                    foreach (var mo in mOrders)
                    {
                        MerchantOrder m = new MerchantOrder()
                        {
                            ReservationId = mo.ReservationId,
                            DeliveryTimeElapse = mo.DeliveryTimeElapse.ToString("dd-MM-yyyy HH:mm:ss"),
                            OrderAmount = mo.OrderAmount,
                            SecuredFundDate = mo.SecuredFundDate.ToString("dd-MM-yyyy HH:mm:ss"),
                            SecuredFundStatus = mo.SecuredFundStatus,
                            VaultCodeStatus = mo.VaultCodeStatus,
                            MerchantOrderNumber = mo.OrderNumber
                        };

                        TimeSpan timeSpan = mo.DeliveryTimeElapse - DateTime.Now;
                        if ((m.VaultCodeStatus == "0") && (timeSpan > TimeSpan.Zero))
                        {
                            m.DeliveryDurationRemaining = String.Format("{0:00}", Math.Truncate(timeSpan.TotalHours)) + ":" + String.Format("{0:00}", timeSpan.Minutes) + ":" + String.Format("{0:00}", timeSpan.Seconds);
                        }
                        else
                        {
                            m.DeliveryDurationRemaining = "00:00:00";
                        }
                        res._MerchantOrders.Add(m);
                    }

                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Failed";
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


        public static GetMerchants GetMerchants()
        {
            CvdbEntities _db = new CvdbEntities();
            GetMerchants res = new GetMerchants();

            try
            {
                var result = _db.Merchants.Where(m => m.MerchantID != "A00000").Select(x => new GetMerchant
                {
                    MerchantID = x.MerchantID,
                    MerchantName = x.MerchantName,
                    MerchantTrxnFee = x.MerchantTrxnFee,
                    MerchantSplits = x.MerchantSplits,
                    MerchantFlag = x.Merchant_Flagged
                });
                res.Merchants = result.ToList();
                res.ResponseCode = "00";
                res.ResponseDescription = "OK";
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

        public static GetMerchantUsers GetMerchantUsers()
        {
            CvdbEntities _db = new CvdbEntities();
            GetMerchantUsers res = new GetMerchantUsers();
            try
            {
                var result = _db.MerchantSupports.Select(x => new MerchantUser
                {
                    MerchantFirstName = x.FirstName,
                    MerchantLastName = x.Lastname,
                    MerchantAdmin = x.MerchantAdmin,
                    MerchantId = x.MerchantID,
                    MerchantMobile = x.MerchantMobile,
                    MerchantUserDispatch = x.MerchantUserDispatch,
                    MerchantUserEmail = x.MerchantUserEmail,
                    MerchantUserReport = x.MerchantUserReport,
                    Restrict_Flag = x.Restrict_Flag
                });
                res.MerchantUsers = result.ToList();
                res.ResponseCode = "00";
                res.ResponseDescription = "OK";
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

        public static GetMerchantUsers GetMerchantUsersByMerchantId(string merchantId)
        {
            CvdbEntities _db = new CvdbEntities();
            GetMerchantUsers res = new GetMerchantUsers();
            try
            {
                var result = _db.MerchantSupports.Where(x => x.MerchantID == merchantId).Select(x => new MerchantUser
                {
                    MerchantAdmin = x.MerchantAdmin.Value,
                    MerchantId = x.MerchantID,
                    MerchantMobile = x.MerchantMobile,
                    MerchantUserDispatch = x.MerchantUserDispatch.Value,
                    MerchantUserEmail = x.MerchantUserEmail,
                    MerchantUserReport = x.MerchantUserReport.Value,
                    Restrict_Flag = x.Restrict_Flag.Value,
                    MerchantFirstName = x.FirstName,
                    MerchantLastName = x.Lastname,
                    MerchantSupportID = x.MerchantSupportID
                });
                res.MerchantUsers = result.ToList();
                res.ResponseCode = "00";
                res.ResponseDescription = "OK";
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

        public static GetAggregators GetAggregators()
        {
            CvdbEntities _db = new CvdbEntities();
            GetAggregators res = new GetAggregators();
            res.Aggregators = new List<GetAggregator>();

            try
            {
                foreach (Aggregator a in _db.Aggregators)
                {
                    GetAggregator ag = new GetAggregator
                    {
                        AggregatorEmail = a.AggregatorEmail,
                        AggregatorId = a.AggregatorID,
                        AggregatorMobile = a.AggregatorMobile,
                        AggregatorName = a.AggregatorName,
                        Restrict_Flag = a.Restrict_Flag.Value,
                        ISO = a.ISO == null ? 0 : a.ISO.Value,
                        SubscribeOff = a.Subscribeoff == null ? 0 : 1
                    };
                    res.Aggregators.Add(ag);
                }

                foreach(GetAggregator ag in res.Aggregators)
                {
                    ag.AggregatorMerchants = _db.AggregatorMerchants.Where(am => am.AggregatorID == ag.AggregatorId).Select(m => new MerchantItem()
                    {
                        MerchantID = m.MerchantID,
                        MerchantName = m.Merchant.MerchantName
                    }).ToList();
                }

                res.ResponseCode = "00";
                res.ResponseDescription = "OK";
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

        public static GetAggregatorMerchantsRes GetAggregatorMerchants(string AggregatorId, string SecureCode)
        {
            CvdbEntities _db = new CvdbEntities();
            GetAggregatorMerchantsRes res = new GetAggregatorMerchantsRes();
            try
            {
                var a = _db.Aggregators.Where(ag => ag.AggregatorID == AggregatorId && ag.SecureCode == SecureCode).FirstOrDefault();

                if (a == null)
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid AggregatorID or Secure Code";
                    return res;
                }

                res.AggregatorMerchants = new List<GetAggregatorMerchant>();

                foreach (AggregatorMerchant am in a.AggregatorMerchants)
                {
                    res.AggregatorMerchants.Add(new GetAggregatorMerchant()
                    {
                        MerchantId = am.MerchantID,
                        MerchantName = am.Merchant.MerchantName
                    });
                }

                res.ResponseCode = "00";
                res.ResponseDescription = "OK";
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

        public static AvailableMerchants GetAvailableMerchants()
        {
            CvdbEntities _db = new CvdbEntities();
            AvailableMerchants res = new AvailableMerchants();
            res.Merchants = new List<MerchantItem>();

            try
            {
                var query = _db.Merchants.Where(m => m.MerchantID != "A00000");

                foreach (Merchant m in query)
                {
                    res.Merchants.Add(new MerchantItem()
                    {
                        MerchantID = m.MerchantID,
                        MerchantName = m.MerchantName
                    });
                }

                res.ResponseCode = "00";
                res.ResponseDescription = "OK";
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



        //Get Merchant Payouts
        //Query Disbursement_tbl
        //Returns all reservation orders excluding Disburse_id,  payment tag  and Self Service Maintenance Flag
        //Selection Criterion: Merchant_id matches prefix of reservation_id within defined date range excluding failed records, group by Quickteller reference
        public static GetDisbursementPayouts GetMerchantPayout(string merchantId, DateTime startDate, DateTime endDate)
        {
            CvdbEntities _db = new CvdbEntities();
            GetDisbursementPayouts res = new GetDisbursementPayouts();
            try
            {
                string prefix = merchantId == "A00000" ? "A" : merchantId;
                var disbursements = (from dd in _db.Disbursments
                                    where dd.ReservationID.StartsWith(prefix)
                                    where dd.DisbursementDate >= startDate
                                    where dd.DisbursementDate <= endDate
                                    select new
                                    {
                                        ReservationID = dd.ReservationID,
                                        MerchantOrderNo = dd.MerchantOrderNumber,
                                        QuickTellerRef = dd.DisbursementQuickTellerRef,
                                        Amount = dd.DisbursementAmount,
                                        Status = dd.DisbursementStatus,
                                        Date = dd.DisbursementDate,
                                        Payment_Tag = dd.Payment_Tag.Trim()
                                    }).ToList();

                var Disbursement = (from dd in disbursements
                                   group dd by dd.QuickTellerRef into am
                                   select new
                                   {
                                       QuickTellerRef = am.Key,
                                       count = am.Count(),
                                       sum = am.Sum(x => x.Amount)
                                   }).ToList();

                if (disbursements != null)
                {
                    var MerchantPayouts = Disbursement.Select(x => new DisbursementPayout
                    {
                        DisburseReference = x.QuickTellerRef,
                        TotalAmount = x.sum.Value,
                        TransactionCount = x.count,
                        DisbusementItems = disbursements.Where(y => y.QuickTellerRef == x.QuickTellerRef).Select(s => new Disbusement
                        {
                            Amount = s.Amount.Value,
                            MerchantOrderNo = s.MerchantOrderNo,
                            ReservationID = s.ReservationID,
                            Status = ServiceHelper.GetStatusString(s.Status),
                            Date = s.Date.HasValue ? s.Date.Value.ToString("dd-MM-yyyy HH:mm:ss") : "",
                            CBNCode = s.Payment_Tag.Substring(s.Payment_Tag.Length - 3)
                        }).ToList()

                    });

                    res.DisbursementPayouts = MerchantPayouts.ToList();
                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Failed";
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

        public static GetFaiedDisbursementPayouts GetFailedMerchantPayout(string merchantId, DateTime startDate, DateTime endDate)
        {
            CvdbEntities _db = new CvdbEntities();
            GetFaiedDisbursementPayouts res = new GetFaiedDisbursementPayouts();
            try
            {
                var disbursements = (from dd in _db.Disbursments
                                    where !(dd.DisbursementStatus == "90000" || dd.DisbursementStatus.EndsWith("09") || dd.DisbursementStatus.EndsWith("A0") || dd.DisbursementStatus.EndsWith("E18") || dd.DisbursementStatus.EndsWith("E19"))
                                    where dd.ReservationID.StartsWith(merchantId)
                                    where dd.DisbursementDate >= startDate
                                    where dd.DisbursementDate <= endDate

                                    select new
                                    {
                                        ReservationID = dd.ReservationID,
                                        MerchantOrderNo = dd.MerchantOrderNumber,
                                        QuickTellerRef = dd.DisbursementQuickTellerRef,
                                        Amount = dd.DisbursementAmount,
                                        Date = dd.DisbursementDate,
                                        Payment_Tag = dd.Payment_Tag.Trim()
                                    }).ToList();

                var Disbursement = from dd in disbursements
                                   group dd by dd.QuickTellerRef into am
                                   select new
                                   {
                                       QuickTellerRef = am.Key,
                                       count = am.Count(),
                                       sum = am.Sum(x => x.Amount)
                                   };

                if (disbursements != null)
                {
                    var MerchantPayouts = Disbursement.Select(x => new FailedDisbursementPayout
                    {
                        DisburseReference = x.QuickTellerRef,
                        TotalAmount = x.sum.Value,
                        TransactionCount = x.count,
                        DisbusementItems = disbursements.Where(y => y.QuickTellerRef == x.QuickTellerRef).Select(s => new FailedDisbursement
                        {
                            Amount = s.Amount.Value,
                            MerchantOrderNo = s.MerchantOrderNo,
                            ReservationID = s.ReservationID,
                            Date = s.Date.HasValue ? s.Date.Value.ToString("dd-MM-yyyy HH:mm:ss") : "",
                            Status = "Failed",
                            CBNCode = s.Payment_Tag.Substring(s.Payment_Tag.Length - 3),
                            AccountNumber = s.Payment_Tag.Substring(0, 10)
                        }).ToList()
                    });

                    res.DisbursementPayouts = MerchantPayouts.ToList();
                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Failed";
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

        //•	Get Merchant Pending Payouts
        //o Query Queued Disbursement_tbl
        //o Returns all columns excluding Qd_id and payment tag
        //  Selection Criterion: Merchant_id matches prefix of reservation_id where Disburse_log_status IS NULL
        public static GetMerchantPendingPayout GetMerchantPendingPayout(string merchantId)
        {
            CvdbEntities _db = new CvdbEntities();
            GetMerchantPendingPayout res = new GetMerchantPendingPayout();
            try
            {
                var disbursements = from qd in _db.QueuedDisbursements
                                    where qd.DisbursementLogStatus == null
                                    where qd.ReservationID.StartsWith(merchantId)

                                    select new MerchantPendingPayout
                                    {
                                        ReservationID = qd.ReservationID,
                                        DisbursementAmount = qd.DisbursmentAmount,
                                        OrderNumber = qd.MerchantOrderNumber
                                    };

                res.MerchantPendingPayouts = disbursements.ToList();
                res.ResponseCode = "00";
                res.ResponseDescription = "OK";
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

        public static MerchantPayoutExceptions GetMerchantPayoutExceptions(string merchantId, DateTime startDate, DateTime endDate)
        {
            CvdbEntities _db = new CvdbEntities();
            MerchantPayoutExceptions res = new MerchantPayoutExceptions();
            res.Exceptions = new List<DisbursementException>();

            try
            {
                var merch = _db.Merchants.Where(m => m.MerchantID == merchantId).FirstOrDefault();
                if (merch != null)
                {
                    var exceptions = (from record in _db.Disbursments
                                     where record.ReservationID.StartsWith(merchantId)
                                     && record.DisbursementDate >= startDate
                                     && record.DisbursementDate <= endDate
                                     && (record.DisbursementStatus.EndsWith("09") || record.DisbursementStatus.EndsWith("A0") || record.DisbursementStatus.EndsWith("E18") || record.DisbursementStatus.EndsWith("E19"))
                                     select new
                                     {
                                         Date = record.DisbursementDate,
                                         OrderAmount = record.DisbursementAmount ?? 0,
                                         DisburseReference = record.DisbursementQuickTellerRef,
                                         ReservationId = record.ReservationID,
                                         CBNCode = record.Payment_Tag.Substring(record.Payment_Tag.Trim().Length - 3)
                                     }).ToList();

                    var disbursementRefs = (from dd in exceptions
                                        group dd by dd.DisburseReference into am
                                        select new
                                        {
                                            DisburseReference = am.Key,
                                            count = am.Count(),
                                            sum = am.Sum(x => x.OrderAmount)
                                        }).ToList();

                    foreach (var disbursementRef in disbursementRefs)
                    {
                        DisbursementException de = new DisbursementException();
                        de.DisburseReference = disbursementRef.DisburseReference;
                        de.TotalAmount = disbursementRef.sum;
                        de.TransactionCount = disbursementRef.count;
                        de.MerchantPayoutExceptions = new List<MerchantPayoutException>();

                        var diburseItems = exceptions.Where(e => e.DisburseReference == disbursementRef.DisburseReference);

                        foreach(var d in diburseItems)

                        de.MerchantPayoutExceptions.Add(new MerchantPayoutException()
                        {
                            Date = d.Date.HasValue ? d.Date.Value.ToString("dd-MM-yyyy HH:mm:ss") : "",
                            OrderAmount = d.OrderAmount,
                            ReservationId = d.ReservationId,
                            CBNCode = d.CBNCode
                        });

                        res.Exceptions.Add(de);
                    }

                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid MerchantID: " + merchantId;
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

        //Get Merchant refunds
        //Query Merchant_refund_tbl
        //Returns all columns excluding table_id & payment_tag
        //Selection Criterion: Merchant_id matches prefix of reservation_id within defined date range excluding failed records, grouped by Quickteller reference
        public static GetMerchantRefunds GetMerchantRefunds(string merchantId, DateTime startDate, DateTime endDate)
        {
            CvdbEntities _db = new CvdbEntities();
            GetMerchantRefunds res = new GetMerchantRefunds();
            try
            {
                var Merchant_Refunds = (from mr in _db.MerchantRefunds
                                       where mr.ReservationID.StartsWith(merchantId)
                                       where mr.MerchantRefundDate >= startDate
                                       where mr.MerchantRefundDate <= endDate

                                       select new
                                       {
                                           ReservationID = mr.ReservationID,
                                           MerchantOrderNo = mr.MerchantOrderNumber,
                                           QuickTellerRef = mr.MerchantQuickTellerRef,
                                           Amount = mr.MerchantRefundAmount,
                                           Status = mr.MerchantRefundStatus,
                                           Date = mr.MerchantRefundDate,
                                           Payment_tag = mr.Payment_Tag.Trim()
                                       }).ToList();

                var GroupedMerchantRefund = (from mr in Merchant_Refunds
                                            group mr by mr.QuickTellerRef into mrs
                                            select new
                                            {
                                                QuickTellerRef = mrs.Key,
                                                count = mrs.Count(),
                                                sum = mrs.Sum(x => x.Amount)
                                            }).ToList();

                if (Merchant_Refunds != null)
                {
                    var MerchantRefundRes = GroupedMerchantRefund.Select(x => new Merchant_Refund
                    {
                        DisburseReference = x.QuickTellerRef,
                        TotalAmount = x.sum.Value,
                        TransactionCount = x.count,
                        RefundItem = Merchant_Refunds.Where(y => y.QuickTellerRef == x.QuickTellerRef).Select(s => new Refund
                        {
                            Amount = s.Amount.Value,
                            MerchantOrderNo = s.MerchantOrderNo,
                            ReservationID = s.ReservationID,
                            Status = ServiceHelper.GetStatusString(s.Status),
                            Date = s.Date.HasValue ? s.Date.Value.ToString("dd-MM-yyyy HH:mm:ss") : "",
                            CbnCode = s.Payment_tag.Substring(s.Payment_tag.Length - 3)
                        }).ToList()
                    });

                    res.MerchantRefunds = MerchantRefundRes.ToList();
                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Failed";
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

        public static MerchantQueuedRefunds GetMerchantQueuedRefunds(string merchantId)
        {
            CvdbEntities _db = new CvdbEntities();
            MerchantQueuedRefunds res = new MerchantQueuedRefunds();
            res.QueuedRefunds = new List<MerchantQueuedRefund>();

            try
            {
                var merch = _db.Merchants.Where(m => m.MerchantID == merchantId).FirstOrDefault();
                if (merch != null)
                {
                    var refunds = _db.QueuedDeliveryRefunds.Where(r => r.ReservationID.StartsWith(merchantId) && r.PayOutStatus == null);

                    foreach (QueuedDeliveryRefund rf in refunds)
                    {
                        res.QueuedRefunds.Add(new MerchantQueuedRefund()
                        {
                            Amount = rf.MerchantRefundAmount ?? 0,
                            MerchantOrderId = rf.MerchantOrderNumber
                        });
                    }

                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid MerchantID: " + merchantId;
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

        public static MerchantRefundExceptions GetMerchantRefundExceptions(string merchantId, DateTime startDate, DateTime endDate)
        {
            CvdbEntities _db = new CvdbEntities();
            MerchantRefundExceptions res = new MerchantRefundExceptions();
            res.Exceptions = new List<RefundException>();

            try
            {
                var merch = _db.Merchants.Where(m => m.MerchantID == merchantId).FirstOrDefault();
                if (merch != null)
                {
                    var exceptions = (from record in _db.MerchantRefunds
                                      where record.ReservationID.StartsWith(merchantId)
                                      && record.MerchantRefundDate >= startDate
                                      && record.MerchantRefundDate <= endDate
                                      && (record.MerchantRefundStatus.EndsWith("09") || record.MerchantRefundStatus.EndsWith("A0") || record.MerchantRefundStatus.EndsWith("E18") || record.MerchantRefundStatus.EndsWith("E19"))
                                      select new
                                      {
                                          Date = record.MerchantRefundDate,
                                          OrderAmount = record.MerchantRefundAmount ?? 0,
                                          RefundReference = record.MerchantQuickTellerRef,
                                          ReservationId = record.ReservationID,
                                          CBNCode = record.Payment_Tag.Substring(record.Payment_Tag.Trim().Length - 3)
                                      }).ToList();

                    var refundRefs = (from dd in exceptions
                                            group dd by dd.RefundReference into am
                                            select new
                                            {
                                                RefundReference = am.Key,
                                                count = am.Count(),
                                                sum = am.Sum(x => x.OrderAmount)
                                            }).ToList();

                    foreach (var refundRef in refundRefs)
                    {
                        RefundException ex = new RefundException();
                        ex.RefundReference = refundRef.RefundReference;
                        ex.TotalAmount = refundRef.sum;
                        ex.TransactionCount = refundRef.count;
                        ex.MerchantRefundExceptions = new List<MerchantRefundException>();

                        var refundItems = exceptions.Where(e => e.RefundReference == refundRef.RefundReference);

                        foreach (var item in refundItems)
                        {
                            ex.MerchantRefundExceptions.Add(new MerchantRefundException()
                            {
                                Date = item.Date.HasValue ? item.Date.Value.ToString("dd-MM-yyyy hh:mm:ss") : "",
                                OrderAmount = item.OrderAmount,
                                ReservationId = item.ReservationId,
                                CBNCode = item.CBNCode
                            });
                        }

                        res.Exceptions.Add(ex);
                    }

                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid MerchantID: " + merchantId;
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



        //o Query Cust_Reversal_tbl
        //o Returns all columns excluding Quickteller reference, cust_id & payment_tag
        //o Criterion: Merchant_id matches prefix of reservation_id within defined date range excluding failed records
        public static GetReversals GetReversals(string merchantId, DateTime startDate, DateTime endDate)
        {
            CvdbEntities _db = new CvdbEntities();
            GetReversals res = new GetReversals();
            try
            {
                var reversals = (from cr in _db.CustomerReversals
                                where cr.ReservationID.StartsWith(merchantId)
                                where cr.ReversalDate >= startDate
                                where cr.ReversalDate <= endDate

                                select new
                                {
                                    ReservationID = cr.ReservationID,
                                    ReversalAmount = cr.ReversalAmount.Value,
                                    ReversalDate = cr.ReversalDate,
                                    ReversalStatus = cr.ReversalStatus.Trim()
                                }).ToList();

                if (reversals != null)
                {
                    res.Reversals = reversals.Select(r => new Reversal()
                    {
                        ReservationID = r.ReservationID,
                        ReversalAmount = r.ReversalAmount,
                        ReversalStatus = ServiceHelper.GetStatusString(r.ReversalStatus),
                        ReversalDate = r.ReversalDate.HasValue ? r.ReversalDate.Value.ToString("dd-MM-yyyy HH:mm:ss") : "",
                    }).ToList();

                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Failed";
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

        public static GetFailedReversals GetFailedReversals(string merchantId, DateTime startDate, DateTime endDate)
        {
            CvdbEntities _db = new CvdbEntities();
            GetFailedReversals res = new GetFailedReversals();
            string prefix = merchantId == "A00000" ? "A" : merchantId;
            try
            {
                var reversals = (from cr in _db.CustomerReversals
                                where cr.ReservationID.StartsWith(prefix)
                                where cr.ReversalDate >= startDate
                                where cr.ReversalDate <= endDate
                                where !(cr.ReversalStatus == "90000" || cr.ReversalStatus.EndsWith("09") || cr.ReversalStatus.EndsWith("A0") || cr.ReversalStatus.EndsWith("E18") || cr.ReversalStatus.EndsWith("E19"))

                                select new 
                                {
                                    ReservationID = cr.ReservationID,
                                    ReversalAmount = cr.ReversalAmount ?? 0,
                                    ReversalDate = cr.ReversalDate,
                                    ReversalStatus = "Failed",
                                    Account_number = cr.Payment_Tag.Substring(0, 10),
                                    Bank_cbn_code = cr.Payment_Tag.Trim().Substring(cr.Payment_Tag.Trim().Length - 3),
                                    QTReference = cr.QuicktellerRef
                                }).ToList();

                if (reversals != null)
                {
                    res.Reversals = reversals.Select(r => new FailedReversal()
                    {
                        ReservationID = r.ReservationID,
                        ReversalAmount = r.ReversalAmount,
                        ReversalStatus = ServiceHelper.GetStatusString(r.ReversalStatus),
                        ReversalDate = r.ReversalDate.HasValue ? r.ReversalDate.Value.ToString("dd-MM-yyyy HH:mm:ss") : "",
                        AccountNumber = r.Account_number,
                        BankCbnCode = r.Bank_cbn_code,
                        QuicktellerRef = r.QTReference
                    }).ToList();

                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Failed";
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

        public static ReversalExceptions GetReversalExceptions(string merchantId, DateTime startDate, DateTime endDate)
        {
            CvdbEntities _db = new CvdbEntities();
            ReversalExceptions res = new ReversalExceptions();
            res.Exceptions = new List<ReversalException>();
            string prefix = merchantId == "A00000" ? "A" : merchantId;

            try
            {
                var merch = _db.Merchants.Where(m => m.MerchantID == merchantId).FirstOrDefault();
                if (merch != null)
                {
                    var exceptions = _db.CustomerReversals.Where(r => r.ReservationID.StartsWith(prefix) && r.ReversalDate > startDate && r.ReversalDate < endDate
                    && (r.ReversalStatus.EndsWith("09") || r.ReversalStatus.EndsWith("A0") || r.ReversalStatus.EndsWith("E18") || r.ReversalStatus.EndsWith("E19")));

                    foreach (CashvaultCore.Data.CustomerReversal r in exceptions)
                    {
                        res.Exceptions.Add(new ReversalException()
                        {
                            Date = r.ReversalDate.HasValue ? r.ReversalDate.Value.ToString("dd-MM-yyyy HH:mm:ss") : "",
                            OrderAmount = r.ReversalAmount ?? 0,
                            ReversalReference = r.QuicktellerRef,
                            ReservationId = r.ReservationID,
                            CBNCode = r.Payment_Tag.Trim().Substring(r.Payment_Tag.Trim().Length - 3)
                        });
                    }

                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid MerchantID: " + merchantId;
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



        //•	Dashboard Reversals
        //o Query Cust_Reversal_tbl
        //o   Returns: Count of reversals for current day
        //o Criterion: Merchant_id matches prefix of reservation_id
        public static DashboardReversals GetDashboardReversals(string merchantId)
        {
            CvdbEntities _db = new CvdbEntities();
            DashboardReversals res = new DashboardReversals();
            string prefix = merchantId == "A00000" ? "A" : merchantId;
            try
            {
                DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                DateTime endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

                var reversals = from cr in _db.CustomerReversals
                                where cr.ReservationID.StartsWith(prefix)
                                where cr.ReversalDate >= startDate
                                where cr.ReversalDate <= endDate

                                select new
                                {
                                    Amount = cr.ReversalAmount
                                };

                res.TotalReversalVolume = reversals.Count();
                res.TotalReversalValue = reversals.Count() > 0 ? reversals.Sum(x => x.Amount.Value) : 0;
                res.ResponseCode = "00";
                res.ResponseDescription = "OK";

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

        //•	Dashboard Pending redemptions
        //o   Query throttle_tbl
        //o Returns: Count of orders where cust vault code is NULL and delivery duration has not expired
        //o   Criterion: Merchant_id matches prefix of reservation_id
        public static DashboardPendingRedemptions GetDashboardPendingRedemptions(string merchantId)
        {
            CvdbEntities _db = new CvdbEntities();
            DashboardPendingRedemptions res = new DashboardPendingRedemptions();
            string prefix = merchantId == "A00000" ? "A" : merchantId;
            try
            {
                DateTime endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

                var PendingRedemptions = from cr in _db.Throttles
                                         where cr.ReservationID.StartsWith(prefix)
                                         where cr.DeliveryTimeElapsed > DateTime.Now && cr.DeliveryTimeElapsed < endDate
                                         where cr.CustomerResponse == null 
                                         select new
                                         {
                                             Amount = cr.FinalMerchantOrderAmount
                                         };

                res.TotalPendingRedemptionVolume = PendingRedemptions.Count();
                res.TotalPendingRedemptionValue = PendingRedemptions.Count() > 0 ? PendingRedemptions.Sum(x => x.Amount.Value) : 0;
                res.ResponseCode = "00";
                res.ResponseDescription = "OK";

                return res;
            }
            catch (Exception ex)
            {
                res.ResponseCode = "09";
                res.ResponseDescription = ex.Message;
                return res;
            }
        }

        //•	Dashboard Payouts
        //o Query Disbursement_tbl
        //o   Returns: Count of orders for current day where status is "90000"
        //o Criterion: Merchant_id matches prefix of reservation_id
        public static DashboardPayouts GetDashboardPayouts(string merchantId)
        {
            CvdbEntities _db = new CvdbEntities();
            DashboardPayouts res = new DashboardPayouts();
            string prefix = merchantId == "A00000" ? "A" : merchantId;
            try
            {
                DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                DateTime endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

                var Payouts = from cr in _db.Disbursments
                              where cr.ReservationID.StartsWith(prefix)
                              where cr.DisbursementDate >= startDate
                              where cr.DisbursementDate <= endDate
                              where cr.DisbursementStatus == "90000"
                              select new
                              {
                                  Amount = cr.DisbursementAmount
                              };

                res.TotalPayoutVolume = Payouts.Count();
                res.TotalPayoutValue = Payouts.Count() > 0 ? Payouts.Sum(x => x.Amount.Value) : 0;
                res.ResponseCode = "00";
                res.ResponseDescription = "OK";

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

        public static DashboardReservations GetDashBoardReservations(string merchantId)
        {
            CvdbEntities _db = new CvdbEntities();
            DashboardReservations res = new DashboardReservations();

            try
            {
                var merch = _db.Merchants.Where(m => m.MerchantID == merchantId).FirstOrDefault();
                if (merch != null)
                {
                    DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                    DateTime endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

                    string prefix = merchantId == "A00000" ? "A" : merchantId;

                    var fs = _db.FundSources.Where(f => f.ReservationID.StartsWith(prefix) && f.FundSecured == "00" && f.FundSecuredDate.Value > startDate && f.FundSecuredDate.Value < endDate);
                    if ((fs != null) && (fs.Count() > 0))
                    {
                        res.TotalReversationVolume = fs.Select(f => f.ReservationID).Distinct().Count();
                    }

                    var fs2 = _db.FundSources.Where(f => f.ReservationID.StartsWith(prefix) && f.FundSecured == "00" && f.FundSecuredDate.Value > startDate && f.FundSecuredDate.Value < endDate);
                    if ((fs2 != null) && (fs2.Count() > 0))
                    {
                        res.TotalReversationValue = fs2.Sum(f2 => f2.MerchantOrderAmount ?? 0);
                    }

                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid MerchantID: " + merchantId;
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



        public static MerchantUserStats GetMerchantUserStats(string merchantId)
        {
            CvdbEntities _db = new CvdbEntities();
            MerchantUserStats res = new MerchantUserStats();
            try
            {
                var merch = _db.Merchants.Where(m => m.MerchantID == merchantId).FirstOrDefault();
                if (merch != null)
                {
                    res.AdminCount = merch.MerchantSupports == null ? 0 : merch.MerchantSupports.Where(ms => ms.MerchantAdmin == 1).Count();
                    res.DispatchCount = merch.MerchantSupports == null ? 0 : merch.MerchantSupports.Where(ms => ms.MerchantUserDispatch == 1).Count();
                    res.ReportCount = merch.MerchantSupports == null ? 0 : merch.MerchantSupports.Where(ms => ms.MerchantUserReport == 1).Count();

                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid MerchantID: " + merchantId;
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

        public static HoldBalanceStatement GetHoldBalanceStatement(string merchantId, DateTime startDate, DateTime endDate)
        {
            CvdbEntities _db = new CvdbEntities();
            HoldBalanceStatement res = new HoldBalanceStatement();
            res.Statements = new List<HoldBalanceItem>();

            try
            {
                var merch = _db.Merchants.Where(m => m.MerchantID == merchantId).FirstOrDefault();
                if (merch != null)
                {
                    var payouts = merch.PendingPayouts.Where(pp => pp.DateCreated > startDate && pp.DateCreated < endDate);
                    foreach (PendingPayout p in payouts)
                    {
                        res.Statements.Add(new HoldBalanceItem()
                        {
                            Amount = p.Amount,
                            Date = p.DateCreated.ToString("dd-MM-yyyy HH:mm:ss"),
                            TrxnType = p.PayoutType
                        });
                    }

                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid MerchantID: " + merchantId;
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

        public static CVRevenueStatements GetCashVaultRevenueStatement(string merchantId, DateTime startDate, DateTime endDate)
        {
            CvdbEntities _db = new CvdbEntities();
            CVRevenueStatements res = new CVRevenueStatements();
            res.Statements = new List<CVRevenueStatement>();

            try
            {
                if (merchantId == "A00000")
                {
                    var CVRevenues = _db.CashVaultRevenues.Where(cv => cv.UpdateTimeStamp > startDate && cv.UpdateTimeStamp < endDate);

                    foreach(CashVaultRevenue cv in CVRevenues)
                    {
                        CVRevenueStatement CVRStat = new CVRevenueStatement()
                        {
                            Amount = cv.Credit == 0 ? cv.Debit.Value : cv.Credit.Value,
                            TrxnType = cv.Credit == 0 ? "Debit" : "Credit",
                            Balance = cv.Balance.Value,
                            Date = cv.UpdateTimeStamp.HasValue ? cv.UpdateTimeStamp.Value.ToString("dd-MM-yyyy HH:mm:ss") : "",
                            Narration = cv.Naration
                        };

                        int temp;
                        if ((int.TryParse(cv.ReservationId.Substring(0, 4), out temp)) || (cv.ReservationId.StartsWith("1319")))
                        {
                            CVRStat.MerchantID = "A00000";
                        }
                        else
                        {
                            CVRStat.MerchantID = cv.ReservationId.Substring(0, 6);
                        }

                        res.Statements.Add(CVRStat);
                    }

                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Access Denied";
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

        public static ReservationRate GetReservationRate(string merchantId, DateTime startDate, DateTime endDate)
        {
            CvdbEntities _db = new CvdbEntities();
            ReservationRate res = new ReservationRate();

            try
            {
                var merch = _db.Merchants.Where(m => m.MerchantID == merchantId).FirstOrDefault();
                if (merch != null)
                {
                    string prefix = merchantId == "A00000" ? "A" : merchantId;
                    res.SuccessfulReservations = _db.FundSources.Where(f => f.ReservationID.StartsWith(prefix) && f.FundSecuredDate >= startDate && f.FundSecuredDate <= endDate && f.FundSecured == "00").Count();
                    res.TotalReservations = _db.Reservations.Where(r => r.ReservationID.StartsWith(prefix) && r.ReservationDate >= startDate && r.ReservationDate <= endDate).Count();

                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid MerchantID: " + merchantId;
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

        public static CumulativeMerchantOrders GetCumulativeMerchantOrders(string merchantId, DateTime startDate, DateTime endDate)
        {
            CvdbEntities _db = new CvdbEntities();
            CumulativeMerchantOrders res = new CumulativeMerchantOrders();

            try
            {
                var merch = _db.Merchants.Where(m => m.MerchantID == merchantId).FirstOrDefault();
                if (merch != null)
                {
                    string prefix = merchantId == "A00000" ? "A" : merchantId;

                    var orders = _db.FundSources.Where(f => f.ReservationID.StartsWith(prefix) && f.FundSecuredDate > startDate && f.FundSecuredDate < endDate && f.FundSecured == "00");

                    if (orders.Count() > 0)
                    {     
                        res.MerchantOrders = orders.Sum(fs => fs.MerchantOrderAmount ?? 0);
                    }

                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid MerchantID: " + merchantId;
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

        public static MerchantFlagStatus GetMerchantFlagStatus(string merchantId)
        {
            CvdbEntities _db = new CvdbEntities();
            MerchantFlagStatus res = new MerchantFlagStatus();

            try
            {
                var merch = _db.Merchants.Where(m => m.MerchantID == merchantId).FirstOrDefault();
                if (merch != null)
                {
                    res.Flag = merch.Merchant_Flagged ?? 0;
                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid MerchantID: " + merchantId;
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

        public static CumulativeCancellations GetCumulativeCancellations(string merchantId, DateTime startDate, DateTime endDate)
        {
            CvdbEntities _db = new CvdbEntities();
            CumulativeCancellations res = new CumulativeCancellations();

            try
            {
                var merch = _db.Merchants.Where(m => m.MerchantID == merchantId).FirstOrDefault();
                if (merch != null)
                {
                    string prefix = merchantId == "A00000" ? "A" : merchantId;

                    var reversals = _db.CustomerReversals.Where(r => r.ReservationID.StartsWith(prefix) && r.ReversalDate > startDate && r.ReversalDate < endDate && r.ReversalStatus.Trim() == "90000");

                    if (reversals.Count() > 0)
                    {
                        res.Cancellations = reversals.Sum(cr => cr.ReversalAmount ?? 0);
                    }

                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid MerchantID: " + merchantId;
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

        public static ReversalRate GetReversalRate(string merchantId, DateTime startDate, DateTime endDate)
        {
            CvdbEntities _db = new CvdbEntities();
            ReversalRate res = new ReversalRate();

            try
            {
                var merch = _db.Merchants.Where(m => m.MerchantID == merchantId).FirstOrDefault();
                if (merch != null)
                {
                    string prefix = merchantId == "A00000" ? "A" : merchantId;
                    res.SecuredFunds = _db.FundSources.Where(f => f.ReservationID.StartsWith(prefix) && f.FundSecuredDate > startDate && f.FundSecuredDate < endDate && f.FundSecured == "00").Count();
                    res.Cancellations = _db.Throttles.Where(r => r.ReservationID.StartsWith(prefix) && r.DeliveryTimeElapsed > startDate && r.DeliveryTimeElapsed < endDate && r.CustomerResponse == null).Count();

                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Invalid MerchantID: " + merchantId;
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

        //added by dharmesh 4.2.1
        public static GetMerchantPendingPayout GetISOPendingPayout(string aggreagatorId, DateTime startDate, DateTime endDate)
        {
            CvdbEntities _db = new CvdbEntities();
            GetMerchantPendingPayout res = new GetMerchantPendingPayout();
            try
            {
                var disbursements = from qr in _db.QueuedISORevenues
                                    where qr.DisburseLogStatus == null
                                    where qr.ReservationID.StartsWith(aggreagatorId) && qr.UpdateTime >= startDate && qr.UpdateTime <= endDate

                                    select new MerchantPendingPayout
                                    {
                                        ReservationID = qr.ReservationID,
                                        DisbursementAmount = qr.RevenueAmount,
                                        OrderNumber = qr.MerchantOrderNo
                                    };

                res.MerchantPendingPayouts = disbursements.ToList();
                res.ResponseCode = "00";
                res.ResponseDescription = "OK";
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

        //added by dharmesh 4.2.2
        public static GetDisbursementPayouts GetISOPayout(string aggreagatorId, DateTime startDate, DateTime endDate)
        {
            CvdbEntities _db = new CvdbEntities();
            GetDisbursementPayouts res = new GetDisbursementPayouts();
            try
            {
                string prefix = aggreagatorId == "AG00000" ? "AG" : aggreagatorId;
                var disbursements = (from dd in _db.DisbursmentISOes
                                     where dd.ReservationID.StartsWith(prefix)
                                     where dd.ISODisbursment >= startDate
                                     where dd.ISODisbursment <= endDate
                                     select new
                                     {
                                         ReservationID = dd.ReservationID,
                                         MerchantOrderNo = dd.MerchantOrderNo,
                                         QuickTellerRef = dd.ISODisbursementQuickTellerRef,
                                         Amount = dd.RevenueAmount,
                                         Status = dd.ISODisbursmentStatus,
                                         Date = dd.ISODisbursment,
                                         Payment_Tag = ""
                                     }).ToList();

                var Disbursement = (from dd in disbursements
                                    group dd by dd.QuickTellerRef into am
                                    select new
                                    {
                                        QuickTellerRef = am.Key,
                                        count = am.Count(),
                                        sum = am.Sum(x => x.Amount)
                                    }).ToList();

                if (disbursements != null)
                {
                    var MerchantPayouts = Disbursement.Select(x => new DisbursementPayout
                    {
                        DisburseReference = x.QuickTellerRef,
                        TotalAmount = x.sum.Value,
                        TransactionCount = x.count,
                        DisbusementItems = disbursements.Where(y => y.QuickTellerRef == x.QuickTellerRef).Select(s => new Disbusement
                        {
                            Amount = s.Amount.Value,
                            MerchantOrderNo = s.MerchantOrderNo,
                            ReservationID = s.ReservationID,
                            Status = ServiceHelper.GetStatusString(s.Status),
                            Date = s.Date.HasValue ? s.Date.Value.ToString("dd-MM-yyyy HH:mm:ss") : "",
                            CBNCode = s.Payment_Tag.Substring(s.Payment_Tag.Length - 3)
                        }).ToList()

                    });

                    res.DisbursementPayouts = MerchantPayouts.ToList();
                    res.ResponseCode = "00";
                    res.ResponseDescription = "OK";
                }
                else
                {
                    res.ResponseCode = "09";
                    res.ResponseDescription = "Failed";
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