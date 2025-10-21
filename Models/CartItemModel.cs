using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Cashere.Models
{
    public class CartItemModel : INotifyPropertyChanged
    {
        private int _quantity;

        public int MenuItemId { get; set; }
        public string ItemName { get; set; }
        public decimal UnitPrice { get; set; }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    RecalculateTotal();
                }
            }
        }

        public decimal SubtotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsTaxable { get; set; }
        public decimal TaxRate { get; set; }

        public void RecalculateTotal()
        {
            SubtotalAmount = UnitPrice * Quantity;
            TaxAmount = IsTaxable ? SubtotalAmount * TaxRate : 0;
            TotalAmount = SubtotalAmount + TaxAmount;
            OnPropertyChanged(nameof(SubtotalAmount));
            OnPropertyChanged(nameof(TaxAmount));
            OnPropertyChanged(nameof(TotalAmount));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
