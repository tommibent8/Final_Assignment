# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file
COPY ../Cryptocop.Software.sln ./

# Copy project file
COPY Cryptocop.Software.Worker.Emails/Cryptocop.Software.Worker.Emails.csproj Cryptocop.Software.Worker.Emails/

# Restore dependencies
RUN dotnet restore Cryptocop.Software.Worker.Emails/Cryptocop.Software.Worker.Emails.csproj

# Copy all source code
COPY . .

# Build the project
WORKDIR /src/Cryptocop.Software.Worker.Emails
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Cryptocop.Software.Worker.Emails.dll"]
