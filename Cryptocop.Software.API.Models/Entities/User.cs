namespace Cryptocop.Software.API.Models.Entities;

public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }

        public ICollection<Address> Addresses { get; set; }
        public ICollection<PaymentCard> PaymentCards { get; set; }
        public ICollection<ShoppingCart> ShoppingCarts { get; set; }
    }

