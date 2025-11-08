# Cryptocop Implementation Plan

## Overview
This is a comprehensive implementation plan for the Cryptocop cryptocurrency sales platform. The project consists of a microservice architecture with 4 main services:
1. **Web API** - Main REST API for the application
2. **Payment Worker Service** - Validates credit cards via AMQP
3. **Email Worker Service** - Sends order confirmation emails via AMQP
4. **RabbitMQ** - Message broker for inter-service communication

## External API
- **Messari API**: https://data.messari.io/docs
- Used for cryptocurrency and exchange data
- Focus on Assets and Markets sections
- Available cryptocurrencies: BTC, ETH, USDT, LINK

---

## Phase 1: Models Layer (5%)

### 1.1 Entity Models
Based on database diagram, ensure all entities have proper properties:
- [x] User (Id, FullName, Email, HashedPassword)
- [x] Address (Id, UserId, StreetName, HouseNumber, ZipCode, Country, City)
- [x] PaymentCard (Id, UserId, CardholderName, CardNumber, Month, Year)
- [x] ShoppingCart (Id, UserId)
- [x] ShoppingCartItem (Id, ShoppingCartId, ProductIdentifier, Quantity, UnitPrice)
- [x] Order (Id, Email, FullName, StreetName, HouseNumber, ZipCode, Country, City, CardholderName, MaskedCreditCard, OrderDate, TotalPrice)
- [x] OrderItem (Id, OrderId, ProductIdentifier, Quantity, UnitPrice, TotalPrice)
- [x] JwtToken (Id, Blacklisted)

### 1.2 DTOs (Data Transfer Objects)
- [ ] ExchangeDto (Id, Name, Slug, AssetSymbol, PriceInUsd, LastTrade)
- [ ] CryptoCurrencyDto (Id, Symbol, Name, Slug, PriceInUsd, ProjectDetails)
- [ ] ShoppingCartItemDto (Id, ProductIdentifier, Quantity, UnitPrice, TotalPrice)
- [ ] AddressDto (Id, StreetName, HouseNumber, ZipCode, Country, City)
- [ ] PaymentCardDto (Id, CardholderName, CardNumber, Month, Year)
- [ ] OrderDto (Id, Email, FullName, StreetName, HouseNumber, ZipCode, Country, City, CardholderName, CreditCard, OrderDate, TotalPrice, OrderItems)
- [ ] OrderItemDto (Id, ProductIdentifier, Quantity, UnitPrice, TotalPrice)
- [ ] UserDto (Id, FullName, Email, TokenId)
- [ ] JwtTokenDto (Token string)

### 1.3 Input Models with Validation
- [ ] RegisterInputModel (Email*, FullName*, Password*, PasswordConfirmation*)
  - Email: Required, valid email
  - FullName: Required, min 3 characters
  - Password: Required, min 8 characters
  - PasswordConfirmation: Required, min 8 characters, must match Password
- [ ] LoginInputModel (Email*, Password*)
  - Email: Required, valid email
  - Password: Required, min 8 characters
- [ ] AddressInputModel (StreetName*, HouseNumber*, ZipCode*, Country*, City*)
  - All required strings
- [ ] PaymentCardInputModel (CardholderName*, CardNumber*, Month, Year)
  - CardholderName: Required, min 3 characters
  - CardNumber: Required, valid credit card
  - Month: Range 1-12
  - Year: Range 0-99
- [ ] OrderInputModel (AddressId, PaymentCardId)
- [ ] ShoppingCartItemInputModel (ProductIdentifier*, Quantity*)
  - ProductIdentifier: Required string
  - Quantity: Required, range 0.01 to float.MaxValue

### 1.4 Envelope Model
- [ ] Generic Envelope<T> for paginated responses

---

## Phase 2: Database Setup (5%)

### 2.1 Database Container
- [x] PostgreSQL container in docker-compose.yml
- [x] Connection string in appsettings.json

### 2.2 DbContext Configuration
- [ ] Configure CryptocopDbContext with all entity mappings
- [ ] Configure relationships between entities
- [ ] Register DbContext in Program.cs

### 2.3 Migrations
- [ ] Create initial migration
- [ ] Apply migration to database
- [ ] Verify database schema matches requirements

---

## Phase 3: Repository Layer (17.5%)

### 3.1 AddressRepository (2.5%)
- [ ] GetAllAddresses(string email) - Get user's addresses
- [ ] AddAddress(Address address) - Add new address
- [ ] DeleteAddress(int id, string email) - Delete user's address

### 3.2 OrderRepository (4%)
- [ ] GetOrders(string email) - Get user's orders
- [ ] CreateNewOrder(string email, int addressId, int paymentCardId) - Create order
  - Retrieve user, address, payment card
  - Create order with masked credit card (************5555)
  - Return order with unmasked card number

### 3.3 PaymentRepository (2%)
- [ ] AddPaymentCard(PaymentCard card) - Add payment card
- [ ] GetStoredPaymentCards(string email) - Get user's payment cards

### 3.4 ShoppingCartRepository (4%)
- [ ] GetCartItems(string email) - Get user's cart items
- [ ] AddCartItem(ShoppingCartItem item) - Add cart item
- [ ] RemoveCartItem(int id) - Remove cart item
- [ ] UpdateCartItemQuantity(int id, float quantity) - Update quantity
- [ ] ClearCart(string email) - Clear user's cart

### 3.5 TokenRepository (2%)
- [ ] CreateNewToken(JwtToken token) - Add token to database
- [ ] IsTokenBlacklisted(int tokenId) - Check if blacklisted
- [ ] VoidToken(int tokenId) - Set token as blacklisted

### 3.6 UserRepository (3%)
- [ ] CreateUser(User user) - Create user with hashed password
  - Check if email exists
  - Hash password using HashingHelper
  - Create new token
  - Return user
- [ ] AuthenticateUser(string email, string password) - Authenticate user
  - Verify credentials
  - Create new token
  - Return user

---

## Phase 4: Service Layer (17.5%)

### 4.1 AccountService (1%)
- [ ] CreateUser(RegisterInputModel) - Register new user
- [ ] AuthenticateUser(LoginInputModel) - Sign in user
- [ ] Logout(int tokenId) - Void JWT token

### 4.2 CryptoCurrencyService (2.5%)
- [ ] GetAvailableCryptocurrencies() - Get BTC, ETH, USDT, LINK
  - Call Messari API
  - Deserialize using HttpResponseMessageExtensions
  - Filter to available cryptocurrencies
  - Return List<CryptoCurrencyDto>

### 4.3 ExchangeService (2.5%)
- [ ] GetExchanges(int pageNumber) - Get paginated exchanges
  - Call Messari API with pagination
  - Deserialize using HttpResponseMessageExtensions
  - Create Envelope<ExchangeDto>
  - Return envelope

### 4.4 JwtTokenService (1%)
- [ ] IsTokenBlacklisted(int tokenId) - Check blacklist status

### 4.5 OrderService (2%)
- [ ] GetOrders(string email) - Get user orders
- [ ] CreateNewOrder(string email, OrderInputModel) - Create order
  - Create order via repository
  - Clear shopping cart
  - Publish to RabbitMQ with routing key 'create-order'

### 4.6 QueueService (2%)
- [ ] PublishMessage(object message, string routingKey) - Publish to RabbitMQ
  - Serialize to JSON
  - Publish using RabbitMQ channel

### 4.7 ShoppingCartService (2.5%)
- [ ] GetCartItems(string email) - Get cart items
- [ ] AddCartItem(string email, ShoppingCartItemInputModel) - Add to cart
  - Call Messari API for current price
  - Deserialize to CryptoCurrencyDto
  - Add to database
- [ ] RemoveCartItem(int id) - Remove item
- [ ] UpdateCartItemQuantity(int id, float quantity) - Update quantity
- [ ] ClearCart(string email) - Clear cart

### 4.8 TokenService (2%)
- [ ] GenerateJwtToken(UserDto) - Generate JWT
  - Create token with user claims
  - Return token string

### 4.9 PaymentService (1%)
- [ ] AddPaymentCard(string email, PaymentCardInputModel) - Add card
- [ ] GetStoredPaymentCards(string email) - Get cards

### 4.10 AddressService (1%)
- [ ] GetAllAddresses(string email) - Get addresses
- [ ] AddAddress(string email, AddressInputModel) - Add address
- [ ] DeleteAddress(int id, string email) - Delete address

---

## Phase 5: API Controllers & Endpoints (10%)

### 5.1 AccountController (3%)
- [ ] POST /api/account/register - Register (no auth required)
- [ ] POST /api/account/signin - Sign in (no auth required)
- [ ] GET /api/account/signout - Sign out (auth required)

### 5.2 ExchangeController (1%)
- [ ] GET /api/exchanges?pageNumber={n} - Get paginated exchanges

### 5.3 CryptoCurrencyController (1%)
- [ ] GET /api/cryptocurrencies - Get BTC, ETH, USDT, LINK

### 5.4 ShoppingCartController (4%)
- [ ] GET /api/cart - Get cart items
- [ ] POST /api/cart - Add cart item
- [ ] DELETE /api/cart/{id} - Remove cart item
- [ ] PATCH /api/cart/{id} - Update quantity
- [ ] DELETE /api/cart - Clear cart

### 5.5 AddressController (2%)
- [ ] GET /api/addresses - Get addresses
- [ ] POST /api/addresses - Add address
- [ ] DELETE /api/addresses/{id} - Delete address

### 5.6 PaymentController (2%)
- [ ] GET /api/payments - Get payment cards
- [ ] POST /api/payments - Add payment card

### 5.7 OrderController (2%)
- [ ] GET /api/orders - Get orders
- [ ] POST /api/orders - Create order

---

## Phase 6: JWT Authentication (10%)

### 6.1 JWT Configuration (1%)
- [x] JWT settings in appsettings.json (Secret, Issuer, Audience)
- [x] JWT authentication setup in Program.cs

### 6.2 JWT Middleware (4%)
- [ ] Implement JwtBlacklistMiddleware
  - Setup OnTokenValidated event
  - Check if token is blacklisted
  - Reject request if blacklisted
- [x] Register middleware in Program.cs

### 6.3 Authorization (2%)
- [ ] Require authentication on all endpoints except:
  - POST /api/account/register
  - POST /api/account/signin
- [ ] Add [Authorize] attribute to controllers
- [ ] Add [AllowAnonymous] to register/signin

---

## Phase 7: Payment Worker Service (10%)

### 7.1 AMQP Listener (2.5%)
- [ ] Setup RabbitMQ connection
- [ ] Create queue 'payment-queue'
- [ ] Bind to routing key 'create-order'
- [ ] Listen for messages

### 7.2 Credit Card Validation (2.5%)
- [ ] Install credit card validation library
- [ ] Validate credit card number from order
- [ ] Print validation result to console

### 7.3 Dockerfile (2.5%)
- [ ] Create Dockerfile for payment worker
- [ ] Use appropriate .NET runtime

### 7.4 Configuration (2.5%)
- [ ] RabbitMQ connection settings in appsettings.json
- [ ] Environment variable support

---

## Phase 8: Email Worker Service (10%)

### 8.1 AMQP Listener (2.5%)
- [ ] Setup RabbitMQ connection
- [ ] Create queue 'email-queue'
- [ ] Bind to routing key 'create-order'
- [ ] Listen for messages

### 8.2 SendGrid Integration (5%)
- [ ] Install SendGrid NuGet package
- [ ] Configure SendGrid API key
- [ ] Create HTML email template
- [ ] Include order details:
  - Customer name
  - Address (street, number, city, zip, country)
  - Order date
  - Total price
  - Order items list
- [ ] Send email on order creation

### 8.3 Dockerfile (2.5%)
- [ ] Create Dockerfile for email worker
- [ ] Use appropriate .NET runtime

---

## Phase 9: RabbitMQ Setup (5%)

### 9.1 RabbitMQ Container
- [ ] Add RabbitMQ to docker-compose.yml
- [ ] Configure management UI
- [ ] Set up exchange and routing keys
- [ ] Configure user credentials

---

## Phase 10: Docker & Docker Compose (10%)

### 10.1 API Dockerfile (5%)
- [ ] Create Dockerfile in root
- [ ] Multi-stage build
- [ ] Restore, build, publish
- [ ] Use appropriate base images

### 10.2 Docker Compose (5%)
- [ ] Complete docker-compose.yml with all services:
  - PostgreSQL (done)
  - RabbitMQ
  - Web API
  - Payment Worker
  - Email Worker
- [ ] Configure service dependencies
- [ ] Configure environment variables
- [ ] Configure networking
- [ ] Configure volumes

---

## Phase 11: Testing & Verification

### 11.1 Database Testing
- [ ] Verify all tables created
- [ ] Test CRUD operations
- [ ] Test relationships

### 11.2 API Testing
- [ ] Test user registration
- [ ] Test user login
- [ ] Test JWT authentication
- [ ] Test all endpoints
- [ ] Test authorization

### 11.3 External API Testing
- [ ] Test cryptocurrency fetch
- [ ] Test exchange fetch
- [ ] Test price updates

### 11.4 Message Queue Testing
- [ ] Test order creation message
- [ ] Test payment worker receives message
- [ ] Test email worker receives message
- [ ] Test credit card validation
- [ ] Test email sending

### 11.5 Integration Testing
- [ ] Test complete order flow
- [ ] Test docker-compose startup
- [ ] Test service communication

---

## Implementation Order

### Priority 1: Foundation
1. Complete Models (DTOs, Input Models, Entities)
2. Database migrations
3. Repository implementations

### Priority 2: Core Functionality
4. Service implementations
5. Controller implementations
6. JWT authentication and middleware

### Priority 3: External Services
7. RabbitMQ setup
8. Payment worker service
9. Email worker service

### Priority 4: Deployment
10. Dockerfiles for all services
11. Complete docker-compose.yml
12. Testing and verification

---

## Notes

### Security Considerations
- Passwords hashed using HashingHelper
- Credit cards masked in database (************5555)
- JWT token blacklisting for logout
- All endpoints require authentication (except register/signin)

### External Dependencies
- Messari API: https://data.messari.io/docs
- SendGrid: https://sendgrid.com/
- RabbitMQ client library
- Credit card validation library

### Configuration Files
- appsettings.json: Database, JWT, RabbitMQ, SendGrid
- docker-compose.yml: All services orchestration
- Dockerfiles: API, Payment Worker, Email Worker

### Key Helpers
- HashingHelper: Password hashing
- PaymentCardHelper: Card masking
- HttpResponseMessageExtensions: API response deserialization

---

## Grading Breakdown

- **Web API: 70%**
  - Authentication using JWT: 10%
  - Endpoints: 10%
  - Service project: 17.5%
  - Repository project: 17.5%
  - Models project: 5%
  - Database: 5%
  - Dockerfile: 5%

- **Payment Service: 10%**
  - AMQP listener: 2.5%
  - Credit card validation: 2.5%
  - Console output: 2.5%
  - Dockerfile: 2.5%

- **Email Service: 10%**
  - AMQP listener: 2.5%
  - SendGrid email: 5%
  - HTML structure: 1%
  - Email content: 4%
  - Dockerfile: 2.5%

- **RabbitMQ: 5%**

- **Docker Compose: 5%**

**Total: 100%**
