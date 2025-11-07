namespace Cryptocop.Software.API.Models.Entities;


public class ShoppingCart
    {
        public int Id { get; set; }

        // FK
        public int UserId { get; set; }
        public User User { get; set; }

        public ICollection<ShoppingCartItem> ShoppingCartItems { get; set; }
    }
