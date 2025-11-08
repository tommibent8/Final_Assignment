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

    public async Task AddPaymentCardAsync(string email, PaymentCardInputModel paymentCard)
    {
        await _paymentRepository.AddPaymentCardAsync(email, paymentCard);
    }

    public async Task<IEnumerable<PaymentCardDto>> GetStoredPaymentCardsAsync(string email)
    {
        return await _paymentRepository.GetStoredPaymentCardsAsync(email);
    }
}