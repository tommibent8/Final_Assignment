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
        // Find user
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) throw new InvalidOperationException("User not found.");

        // Create new card
        var card = new PaymentCard
        {
            CardholderName = paymentCard.CardholderName,
            CardNumber = paymentCard.CardNumber,
            Month = paymentCard.Month,
            Year = paymentCard.Year,
            UserId = user.Id
        };

        _dbContext.PaymentCards.Add(card);
        await _dbContext.SaveChangesAsync();    }

    public async Task<IEnumerable<PaymentCardDto>> GetStoredPaymentCardsAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) throw new InvalidOperationException("User not found.");

        return await _dbContext.PaymentCards
            .Where(c => c.UserId == user.Id)
            .Select(c => new PaymentCardDto
            {
                Id = c.Id,
                CardholderName = c.CardholderName,
                CardNumber = c.CardNumber,
                Month = c.Month,
                Year = c.Year
            })
            .ToListAsync();    }
}