using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cashere.Models
{
    public class DailySummaryResponse
    {
        public DateTime SummaryDate { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalTax { get; set; }
        public decimal CashCollected { get; set; }
        public decimal QRISCollected { get; set; }
        public int TotalItemsSold { get; set; }
    }
}
