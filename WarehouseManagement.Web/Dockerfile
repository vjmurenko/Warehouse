FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY WarehouseManagement.Web/*.csproj ./WarehouseManagement.Web/
COPY WarehouseManagement.Application/*.csproj ./WarehouseManagement.Application/
COPY WarehouseManagement.Domain/*.csproj ./WarehouseManagement.Domain/
COPY WarehouseManagement.Infrastructure/*.csproj ./WarehouseManagement.Infrastructure/

RUN dotnet restore ./WarehouseManagement.Web/WarehouseManagement.Web.csproj

COPY WarehouseManagement.Web/ ./WarehouseManagement.Web/
COPY WarehouseManagement.Application/ ./WarehouseManagement.Application/
COPY WarehouseManagement.Domain/ ./WarehouseManagement.Domain/
COPY WarehouseManagement.Infrastructure/ ./WarehouseManagement.Infrastructure/

RUN dotnet publish ./WarehouseManagement.Web/WarehouseManagement.Web.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

COPY WarehouseManagement.Web/appsettings.Container.json appsettings.Container.json

EXPOSE 80

ENTRYPOINT ["dotnet", "WarehouseManagement.Web.dll"]