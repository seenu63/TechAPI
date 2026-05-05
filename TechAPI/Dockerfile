# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy only csproj first
COPY TechAPI/TechAPI.csproj ./TechAPI/
RUN dotnet restore ./TechAPI/TechAPI.csproj

# Copy full project
COPY TechAPI/. ./TechAPI/
WORKDIR /app/TechAPI

RUN dotnet publish -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TechAPI.dll"]