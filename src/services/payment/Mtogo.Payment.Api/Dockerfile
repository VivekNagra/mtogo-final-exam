FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore src/services/payment/Mtogo.Payment.Api/Mtogo.Payment.Api.csproj
RUN dotnet publish src/services/payment/Mtogo.Payment.Api/Mtogo.Payment.Api.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .

ENTRYPOINT ["dotnet", "Mtogo.Payment.Api.dll"]