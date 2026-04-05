FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY SportsStore.sln .
COPY src/Shared.Contracts/ src/Shared.Contracts/
COPY src/Shipping.Service/ src/Shipping.Service/
RUN dotnet publish src/Shipping.Service/Shipping.Service.csproj -c Release -o /app/publish --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Shipping.Service.dll"]
