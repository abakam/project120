using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashvaultCore.Core
{
    public class Global
    {
        public static string CUSTOMER_REVERSAL_QTPREFIX = ConfigurationManager.AppSettings["CUSTOMER_REVERSALFTCodePrefix"];
        public static string MERCHANT_REFUND_QTPREFIX = ConfigurationManager.AppSettings["MERCHANT_REFUNDFTCodePrefix"];
        public static string DISBURSEMENT_QTPREFIX = ConfigurationManager.AppSettings["DISBURSEMENT_FTCodePrefix"];
        public static int TransferCodeLength = Convert.ToInt32(ConfigurationManager.AppSettings["FTCodeLength"]);
        public static string QTServiceMode = ConfigurationManager.AppSettings["QTServiceMode"];
        public static string DISBURSEMENTISO_QTPREFIX = ConfigurationManager.AppSettings["DISBURSEMENTISO_FTCodePrefix"];
    }
}
