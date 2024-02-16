using CashvaultCore.Data;
using CashVaultService.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace CashVaultService
{
    public static class ServiceHelper
    {

        internal static ReservationRequestRes ValidateReservationRequest(ReservationRequest request)
        {
            ReservationRequestRes response = new ReservationRequestRes();

            //1. Amount in request must be greater than min order set for Merchant id (merchant)
            //2. Sum of percentage of amount sharing across the different entities must be 100
            //3. Count of splits must be equal to or less than what is specified in merchant_tbl
            //4. If Delivery duration is NULL default delivery duration of 7(168 hours) days is set 
            //4.1 Whatever value provided in delivery fee is ignored if delivery duration is NULL  
            //5. If no split is specified, do not return an error go ahead and process.

            var merchant = DataHelper.GetMerchantFromReservationId(request.ReservationId);

            if (merchant == null)
            {
                response.ResponseCode = "09";
                response.ResponseDescription = "Error: Invalid Merchant details in reservation request: " + request.ReservationId;
                return response;
            }

            CvdbEntities _db = new CvdbEntities();
            bool reservationExists = _db.Reservations.Where(r => r.ReservationID == request.ReservationId).Count() > 0;
            _db.Dispose();

            if (reservationExists)
            {
                response.ResponseCode = "09";
                response.ResponseDescription = "Error: Reservation ID already exists " + request.ReservationId;
                return response;
            }           

            if (request.OrderAmount < merchant.MerchantMinOrderAmount)
            {
                response.ResponseCode = "09";
                response.ResponseDescription = "Error: Order amount cannot be less than: " + merchant.MerchantMinOrderAmount;
                return response;
            }

            var Streetz = DataHelper.GetMerchantIdFromReservationId(request.ReservationId);
            if ((Streetz != "A00001" ) && (request.DeliveryFee < 150) && (request.RefundDelivery == 1))
            {
                response.ResponseCode = "09";
                response.ResponseDescription = "Error: Delivery fee must be greater than: " + request.DeliveryFee;
                return response;
            }

            if (request.DeliveryDuration < 1)
            {
                response.ResponseCode = "09";
                response.ResponseDescription = "Error: Delivery Duration cannot be less than an hour";
                return response;
            }            

            if ((merchant.KYC == null || merchant.KYC == 0) && request.OrderAmount > 10000)
            {
                response.ResponseCode = "09";
                response.ResponseDescription = "Error: Merchant KYC still pending, upload KYC docs to remove restriction";
                return response;
            }            

            if (merchant.KYC == 1 && request.OrderAmount > 10000)
            {
                response.ResponseCode = "09";
                response.ResponseDescription = "Error: Merchant audit in progress";
                return response;
            }            

            try
            {
                var merchantSplits = merchant.MerchantSplits ?? 0;
                var reservationRequestSplits = request.ReservationRequestSplits?.Count() ?? 0;
                if (reservationRequestSplits > merchantSplits)
                {
                    response.ResponseCode = "09";
                    response.ResponseDescription = "Error: Specified splits is greater than number allowed Max Split number of :" + merchant.MerchantSplits;
                    return response;
                }
            }
            catch (Exception)
            {
                throw new Exception("Error: Validating Request Splits Caused an Error");
            }

            decimal totalReservationSplitPercentage = 0;

            if (request.ReservationRequestSplits == null)
            {
                response.ResponseCode = "00";
                return response;
            }

            foreach (var requestSplit in request.ReservationRequestSplits)
            {
                if (requestSplit.SplitPercentage < Convert.ToDecimal(0.1))
                {
                    response.ResponseCode = "09";
                    response.ResponseDescription = "Error:Split percentage cannot be less than 0.1. Requested: " + requestSplit.SplitPercentage;
                    return response;
                }
                var vals = requestSplit.SplitPercentage.ToString(CultureInfo.InvariantCulture).Split('.');
                if (vals.Length > 1)
                {
                    if (vals[1].Length > 5)
                    {
                        response.ResponseCode = "09";
                        response.ResponseDescription = "Error:Split percentage cannot be more than 5 decimal places. Requested: " + requestSplit.SplitPercentage;
                        return response;
                    }
                }

                totalReservationSplitPercentage += requestSplit.SplitPercentage;
            }

            if (totalReservationSplitPercentage == 100)
            {
                response.ResponseCode = "00";
                return response;
            }

            response.ResponseCode = "09";
            response.ResponseDescription = "Error: Sum of all splits must equal to 100. Split sum is :" + totalReservationSplitPercentage;
            return response;
        }

        internal static bool ValidateVaultCode(char[] inVaultCode, char[] dbVaultCode, out string vaultCodeCheckResult)
        {
            vaultCodeCheckResult = string.Empty;

            if (inVaultCode.Length != dbVaultCode.Length)
            {
                vaultCodeCheckResult = "Invalid VaultCode Length";
                return false;
            }

            int vaultCodeCheckerInt;
            if (!int.TryParse(ToString(inVaultCode), out vaultCodeCheckerInt))
            {
                vaultCodeCheckResult = "Vaultcode must be a numeric value: " + ToString(inVaultCode);
                return false;
            }

            var vaultCodeCheckSum = int.Parse(ToString(inVaultCode)) + int.Parse(ToString(dbVaultCode));
            if (vaultCodeCheckSum == 1111110)
            {
                vaultCodeCheckResult += "000000";
            }
            else
            {
                vaultCodeCheckResult = "Invalid Vaultcode : " + vaultCodeCheckSum;
                return false;
            }

            return true;
        }

        private static string ToString(IEnumerable<char> charArray)
        {
            return charArray.Aggregate(string.Empty, (current, c) => current + c);
        }

        public static string GenerateRandomString(int length)
        {
            string result = null;

            const string chars = "a0fB#deFnGHi1JokLMn3OpQ4Rs7TUv8WxYz9c";
            RNGCryptoServiceProvider prv = new RNGCryptoServiceProvider();
            byte[] indexes = new byte[length];
            prv.GetNonZeroBytes(indexes);
            for (int i = 0; i < indexes.Length; i++)
            {
                int nextIndex = (int)indexes[i];
                if (nextIndex >= chars.Length)
                {
                    nextIndex = nextIndex % chars.Length;
                }
                result = result + chars[nextIndex];
            }

            prv.Dispose();
            return result;
        }

        public static string GetStatusString(string status)
        {
            string resultCode = "";
            if (status.Trim() == "90000")
            {
                resultCode = "Success";
            }
            else if (status.Trim().EndsWith("09") || status.Trim().EndsWith("A0") || status.Trim().EndsWith("E18") || status.Trim().EndsWith("E19"))
            {
                resultCode = "Pending";
            }
            else
            {
                resultCode = "Failed";
            }

            return resultCode;
        }
    }
}