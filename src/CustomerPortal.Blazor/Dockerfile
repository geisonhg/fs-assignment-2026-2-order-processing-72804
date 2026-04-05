FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY SportsStore.sln .
COPY src/Shared.Contracts/ src/Shared.Contracts/
COPY src/CustomerPortal.Blazor/ src/CustomerPortal.Blazor/
RUN dotnet publish src/CustomerPortal.Blazor/CustomerPortal.Blazor.csproj -c Release -o /app/publish --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CustomerPortal.Blazor.dll"]
