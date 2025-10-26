using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cashere.Models
{
    public class TransactionModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public string PaymentMethod { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal ChangeAmount { get; set; }
        public int ItemCount { get; set; }
        public string Status { get; set; }
        public string CashierName { get; set; }
    }
}
