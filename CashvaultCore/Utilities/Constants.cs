using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace CashvaultCore.Utilities
{
    public static class Constants
    {
        public const string SuccessResponseCode = "00";
        public const string FailureResponseCode = "09";
        public const string InputValidationResponseCode = "02";
        public const string ExceptionResponseCode = "01";
        public const string InvalidMerchantIdMessage = "Invalid Merchant Id";
        public const string RequiredMerchantIdMessage = "Merchant Id is required";
        public const string RequiredAccountNumberMessage = "Account Number is required";
        public const string InvalidBankCbnCodeMessage = "Bank Cbn Code must be 3 digits long";
        public const string RequiredBankCbnCodeMessage = "Bank Cbn code is required";
        public const string RequiredSecureCodeMessage = "Secure Code is required";
        public const string InvalidAmountMessage = "Invalid amount. Amount must be greater than 0";
        public const string RequiredAmountMessage = "Amount is required";
        public const string MerchantNotFound = "Merchant not found";
        public const string AmountLessThanDisbursementAmount = "Value less than minimum disbursement amount";
        public const string BalanceInsufficientForPayout = "Unable to process. Confirm balance can cover payout amount";
        public const string InvalidAccountNumber = "Invalid account number. Account number must be 10 digits long";
        public const string InvalidMobileMessage = "Invalid format string length should be 11";
        public const string RequiredMobileMessage = "Mobile number is required";
        public const string RequiredEmailMessage = "Email is required";
        public const string InvalidEmailMessage = "Invalid email address provided";
    }
}
