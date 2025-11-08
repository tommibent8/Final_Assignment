using System.Text;
using Cryptocop.Software.API.Middleware;
using Cryptocop.Software.API.Repositories.Contexts;
using Cryptocop.Software.API.Repositories.Implementations;
using Cryptocop.Software.API.Repositories.Interfaces;
using Cryptocop.Software.API.Services.Implementations;
using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// CORS (for Swagger/browser requests)
builder.Services.AddCors(policy =>
    policy.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Database
Console.WriteLine("DB Connection: " + builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddDbContext<CryptocopDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IQueueService, QueueService>();

// HttpClient for Messari API (with API key header)
var messariApiKey = builder.Configuration["Messari:ApiKey"];
builder.Services.AddHttpClient<ICryptoCurrencyService, CryptoCurrencyService>(client =>
{
    client.DefaultRequestHeaders.Add("x-messari-api-key", messariApiKey);
});
builder.Services.AddHttpClient<IExchangeService, ExchangeService>(client =>
{
    client.DefaultRequestHeaders.Add("x-messari-api-key", messariApiKey);
});

// JWT setup
var jwt = builder.Configuration.GetSection("JwtSettings");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = key
        };

        // OnTokenValidated event to check if token is blacklisted
        o.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jwtTokenService = context.HttpContext.RequestServices.GetRequiredService<IJwtTokenService>();
                var tokenIdClaim = context.Principal?.Claims.FirstOrDefault(c => c.Type == "tokenId");

                if (tokenIdClaim != null && int.TryParse(tokenIdClaim.Value, out var tokenId))
                {
                    var isBlacklisted = await jwtTokenService.IsTokenBlacklistedAsync(tokenId);
                    if (isBlacklisted)
                    {
                        context.Fail("Token is blacklisted");
                    }
                }
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Pipeline
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseMiddleware<JwtBlacklistMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();
