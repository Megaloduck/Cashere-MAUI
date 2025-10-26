using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cashere.Models
{
    public class TopSellingItemResponse
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public decimal AveragePrice { get; set; }
    }
}