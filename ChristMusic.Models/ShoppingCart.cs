using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ChristMusic.Models
{
    public class ShoppingCart
    {
        //NB. In this class, each combinaison of userId, ProductId and Count has they own record in the shopping Cart table in the database
        //To retrieve all the products that a user has inserted in the shopping cart, we will get all the record that has the id of the user in the ApplicationUserId field and collect all the corresponding productId

        public ShoppingCart()
        {
            Count = 1;
        }
        [Key]
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Range(1,1000,ErrorMessage ="Please enter a value between 1 and 1000")]
        public int Count { get; set; }

        [NotMapped]
        public double Price { get; set; }
    }
}
