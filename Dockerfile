FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Solucion_Pagos_Moviles/AbstractDataAccess/AbstractDataAccess.csproj Solucion_Pagos_Moviles/AbstractDataAccess/
COPY Solucion_Pagos_Moviles/Services/Services.csproj Solucion_Pagos_Moviles/Services/
COPY Solucion_Pagos_Moviles/Entities/Entities.csproj Solucion_Pagos_Moviles/Entities/
COPY Solucion_Pagos_Moviles/Solucion_Pagos_Moviles/Solucion_Pagos_Moviles.csproj Solucion_Pagos_Moviles/Solucion_Pagos_Moviles/

RUN dotnet restore Solucion_Pagos_Moviles/Solucion_Pagos_Moviles/Solucion_Pagos_Moviles.csproj

COPY Solucion_Pagos_Moviles/AbstractDataAccess/ Solucion_Pagos_Moviles/AbstractDataAccess/
COPY Solucion_Pagos_Moviles/Services/ Solucion_Pagos_Moviles/Services/
COPY Solucion_Pagos_Moviles/Entities/ Solucion_Pagos_Moviles/Entities/
COPY Solucion_Pagos_Moviles/Solucion_Pagos_Moviles/ Solucion_Pagos_Moviles/Solucion_Pagos_Moviles/

WORKDIR /src/Solucion_Pagos_Moviles/Solucion_Pagos_Moviles
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Solucion_Pagos_Moviles.dll"]
