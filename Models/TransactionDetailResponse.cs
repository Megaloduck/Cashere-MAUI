using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cashere.Models
{
    public class TransactionDetailResponse
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string PaymentMethod { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public string Status { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
