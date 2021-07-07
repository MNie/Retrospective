FROM mcr.microsoft.com/dotnet/sdk:5.0 AS publish

WORKDIR /src
COPY . .

RUN apt-get update -yq \
    && apt-get install curl gnupg -yq \
    && curl -sL https://deb.nodesource.com/setup_10.x | bash \
    && apt-get install nodejs -yq

# Copy csproj and restore as distinct layers
RUN dotnet restore "./Facade/WebApp/WebApp.csproj"
WORKDIR "/src/Fasade/WebApp"
RUN dotnet publish -c Release -o /app

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0

WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WebApp.dll"]