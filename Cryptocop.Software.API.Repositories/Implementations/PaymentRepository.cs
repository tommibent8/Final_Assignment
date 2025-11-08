using Cryptocop.Software.API.Models.Dtos;
using Cryptocop.Software.API.Models.Entities;
using Cryptocop.Software.API.Models.InputModels;
using Cryptocop.Software.API.Repositories.Contexts;
using Cryptocop.Software.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cryptocop.Software.API.Repositories.Implementations;

public class PaymentRepository : IPaymentRepository
{
    private readonly CryptocopDbContext _dbContext;

    public PaymentRepository(CryptocopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddPaymentCardAsync(string email, PaymentCardInputModel paymentCard)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var newCard = new PaymentCard
        {
            UserId = user.Id,
            CardholderName = paymentCard.CardholderName,
            CardNumber = paymentCard.CardNumber,
            Month = paymentCard.Month,
            Year = paymentCard.Year
        };

        _dbContext.PaymentCards.Add(newCard);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<PaymentCardDto>> GetStoredPaymentCardsAsync(string email)
    {
        var paymentCards = await _dbContext.PaymentCards
            .Where(pc => pc.User.Email == email)
            .Select(pc => new PaymentCardDto
            {
                Id = pc.Id,
                CardholderName = pc.CardholderName,
                CardNumber = pc.CardNumber,
                Month = pc.Month,
                Year = pc.Year
            })
            .ToListAsync();

        return paymentCards;
    }
}