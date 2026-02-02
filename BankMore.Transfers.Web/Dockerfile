FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BankMore.Transfers.Web/BankMore.Transfers.Web.csproj", "BankMore.Transfers.Web/"]
RUN dotnet restore "BankMore.Transfers.Web/BankMore.Transfers.Web.csproj"
COPY . .
WORKDIR "/src/BankMore.Transfers.Web"
RUN dotnet build "./BankMore.Transfers.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BankMore.Transfers.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BankMore.Transfers.Web.dll"]
