using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Services.Interfaces;

namespace Cryptocop.Software.API.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }
    public Task AddPaymentCardAsync(string email, PaymentCardInputModel paymentCard)
    {
        return _paymentRepository.AddPaymentCardAsync(email, paymentCard);
    }

    public Task<IEnumerable<PaymentCardDto>> GetStoredPaymentCardsAsync(string email)
    {
        return _paymentRepository.GetStoredPaymentCardsAsync(email);
    }
}