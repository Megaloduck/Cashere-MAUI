using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cashere.Models
{
    public class ProcessPaymentRequest
    {
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; } // "Cash" or "QRIS"
        public decimal AmountPaid { get; set; }
    }
}
