using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChristMusic.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ListOfShoppingCart { get; set; } //NB.Each shoppingCart contains the combinaison of userId, ProductId (a product that the user has inserted in the cart)
        public OrderHeader OrderHeader { get; set; }
    }
}
