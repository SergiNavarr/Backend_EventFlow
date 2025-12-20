FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Backend_EventFlow/Backend_EventFlow.csproj", "Backend_EventFlow/"]
COPY ["Datos/Datos.csproj", "Datos/"]
COPY ["Negocio/Negocio.csproj", "Negocio/"]

RUN dotnet restore "Backend_EventFlow/Backend_EventFlow.csproj"

COPY . .

WORKDIR "/src/Backend_EventFlow"
RUN dotnet publish "Backend_EventFlow.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Backend_EventFlow.dll"]