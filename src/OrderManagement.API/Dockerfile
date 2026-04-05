FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY SportsStore.sln .
COPY src/Shared.Contracts/ src/Shared.Contracts/
COPY src/OrderManagement.API/ src/OrderManagement.API/
RUN dotnet publish src/OrderManagement.API/OrderManagement.API.csproj -c Release -o /app/publish --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderManagement.API.dll"]
