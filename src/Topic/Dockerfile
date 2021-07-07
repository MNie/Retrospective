FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS publish

WORKDIR /src
COPY . .
# Copy csproj and restore as distinct layers
RUN dotnet restore "./Topic/WebApi/WebApi.fsproj"
WORKDIR "/src/Topic/WebApi"
RUN dotnet publish -c Release -o /app

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine

WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WebApi.dll"]