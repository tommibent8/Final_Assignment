using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class PaymentService : IPaymentService
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