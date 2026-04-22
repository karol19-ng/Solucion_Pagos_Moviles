FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AbstractDataAccess/AbstractDataAccess.csproj AbstractDataAccess/
COPY Services/Services.csproj Services/
COPY Entities/Entities.csproj Entities/
COPY Solucion_Pagos_Moviles/Solucion_Pagos_Moviles.csproj Solucion_Pagos_Moviles/

RUN dotnet restore Solucion_Pagos_Moviles/Solucion_Pagos_Moviles.csproj

COPY AbstractDataAccess/ AbstractDataAccess/
COPY Services/ Services/
COPY Entities/ Entities/
COPY Solucion_Pagos_Moviles/ Solucion_Pagos_Moviles/

WORKDIR /src/Solucion_Pagos_Moviles
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Solucion_Pagos_Moviles.dll"]
