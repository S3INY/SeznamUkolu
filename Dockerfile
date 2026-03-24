# 1. Fáze: Sestavení (Build)
# Použijeme SDK obraz od Microsoftu pro kompilaci kódu
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Zkopírujeme soubory projektu a stáhneme knihovny
COPY . .
RUN dotnet restore

# Zkompilujeme aplikaci do složky /app
RUN dotnet publish -c Release -o /app

# 2. Fáze: Běh (Runtime)
# Použijeme lehčí obraz jen pro spuštění (bez zbytečného balastu)
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

# Zkopírujeme zkompilované soubory z první fáze
COPY --from=build /app .

# Řekneme Dockeru, jakou .dll knihovnu má spustit
# POZOR: Pokud se tvůj projekt jmenuje jinak, změň "MujWebBackend.dll" na svůj název
ENTRYPOINT ["dotnet", "MujWebBackend.dll"]