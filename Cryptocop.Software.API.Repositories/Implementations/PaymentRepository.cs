using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Interfaces;

namespace Cryptocop.Software.API.Repositories.Implementations;

public class PaymentRepository : IPaymentRepository
{
    public Task AddPaymentCardAsync(string email, PaymentCardInputModel paymentCard)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<PaymentCardDto>> GetStoredPaymentCardsAsync(string email)
    {
        throw new NotImplementedException();
    }
}