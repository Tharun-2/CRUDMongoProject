# Step 1: Use official .NET 7 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /app

# Restore dependencies
COPY ["src/CRUDMongo/CRUDMongo.csproj", "CRUDMongo/"]
RUN dotnet restore "CRUDMongo/CRUDMongo.csproj"

# Copy everything else and build
COPY ["src/CRUDMongo/", "CRUDMongo/"]
WORKDIR /app/CRUDMongo
RUN dotnet build "CRUDMongo.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "CRUDMongo.csproj" -c Release -o /app/publish

# Step 2: Use a runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final

WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5001
EXPOSE 5001

ENTRYPOINT ["dotnet", "CRUDMongo.dll"]
