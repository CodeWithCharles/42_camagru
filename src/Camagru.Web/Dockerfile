# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY Camagru.slnx ./
COPY src/Camagru.Domain/Camagru.Domain.csproj src/Camagru.Domain/
COPY src/Camagru.Application/Camagru.Application.csproj src/Camagru.Application/
COPY src/Camagru.Infrastructure/Camagru.Infrastructure.csproj src/Camagru.Infrastructure/
COPY src/Camagru.Web/Camagru.Web.csproj src/Camagru.Web/

# Restore dependencies
RUN dotnet restore src/Camagru.Web/Camagru.Web.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish src/Camagru.Web/Camagru.Web.csproj \
    --configuration Release \
    --output /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create uploads directory
RUN mkdir -p wwwroot/uploads

COPY --from=build /app/publish .

EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "Camagru.Web.dll"]