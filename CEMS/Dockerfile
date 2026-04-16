# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["CEMS.csproj", "./"]
RUN dotnet restore "CEMS.csproj"

COPY . .
RUN dotnet publish "CEMS.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

EXPOSE 10000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CEMS.dll"]