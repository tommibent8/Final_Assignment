# Use .NET SDK to build the project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and restore dependencies
COPY ["Cryptocop.Software.Worker.Payments/Cryptocop.Software.Worker.Payments.csproj", "Cryptocop.Software.Worker.Payments/"]
RUN dotnet restore "Cryptocop.Software.Worker.Payments/Cryptocop.Software.Worker.Payments.csproj"

# Copy all source files
COPY . .

# Build and publish
WORKDIR "/src/Cryptocop.Software.Worker.Payments"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables (optional defaults)
ENV RabbitMq__HostName=rabbitmq
ENV RabbitMq__UserName=guest
ENV RabbitMq__Password=guest
ENV RabbitMq__ExchangeName=cryptocop-exchange
ENV RabbitMq__QueueName=payment-queue
ENV RabbitMq__RoutingKey=create-order

ENTRYPOINT ["dotnet", "Cryptocop.Software.Worker.Payments.dll"]
