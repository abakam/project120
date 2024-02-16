using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CashVaultService.Models
{
    public class ReservationRate : Response
    {
        public int SuccessfulReservations { get; set; }
        public int TotalReservations { get; set; }
    }
}