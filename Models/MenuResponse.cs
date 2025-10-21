using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Cashere.Models
{
    public class MenuCategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public List<MenuItemResponse> Items { get; set; } = new();
    }

    public class MenuItemResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsTaxable { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class TaxSettingsResponse
    {
        public int Id { get; set; }
        public decimal DefaultTaxRate { get; set; }
        public string TaxName { get; set; }
        public bool IsEnabled { get; set; }
    }
}
