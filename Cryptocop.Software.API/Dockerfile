# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file
COPY ../Cryptocop.Software.sln ./

# Copy all project files
COPY Cryptocop.Software.API/Cryptocop.Software.API.csproj Cryptocop.Software.API/
COPY Cryptocop.Software.API.Models/Cryptocop.Software.API.Models.csproj Cryptocop.Software.API.Models/
COPY Cryptocop.Software.API.Repositories/Cryptocop.Software.API.Repositories.csproj Cryptocop.Software.API.Repositories/
COPY Cryptocop.Software.API.Services/Cryptocop.Software.API.Services.csproj Cryptocop.Software.API.Services/

# Restore dependencies
RUN dotnet restore Cryptocop.Software.API/Cryptocop.Software.API.csproj

# Copy all source code
COPY . .

# Build the API project
WORKDIR /src/Cryptocop.Software.API
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Cryptocop.Software.API.dll"]