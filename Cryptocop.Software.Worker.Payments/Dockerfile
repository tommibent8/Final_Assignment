# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file
COPY ../Cryptocop.Software.sln ./

# Copy project file
COPY Cryptocop.Software.Worker.Payments/Cryptocop.Software.Worker.Payments.csproj Cryptocop.Software.Worker.Payments/

# Restore dependencies
RUN dotnet restore Cryptocop.Software.Worker.Payments/Cryptocop.Software.Worker.Payments.csproj

# Copy all source code
COPY . .

# Build the project
WORKDIR /src/Cryptocop.Software.Worker.Payments
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Cryptocop.Software.Worker.Payments.dll"]
