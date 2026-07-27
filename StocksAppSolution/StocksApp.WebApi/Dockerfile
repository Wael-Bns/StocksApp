FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["StocksApp.WebApi/StocksApp.WebApi.csproj", "StocksApp.WebApi/"]
COPY ["StocksApp.Core/StocksApp.Core.csproj", "StocksApp.Core/"]
COPY ["StocksApp.Infrastructure/StocksApp.Infrastructure.csproj", "StocksApp.Infrastructure/"]
RUN dotnet restore "./StocksApp.WebApi/StocksApp.WebApi.csproj"
COPY . .
WORKDIR "/src/StocksApp.WebApi"
RUN dotnet build "./StocksApp.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

RUN dotnet tool install --global dotnet-ef --version 8.*
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet ef migrations bundle \
    --project "../StocksApp.Infrastructure" \
    --startup-project "." \
    --self-contained -r linux-x64 \
    -o /app/efbundle

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./StocksApp.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /app/efbundle ./efbundle

ENTRYPOINT ["dotnet", "StocksApp.WebApi.dll"]