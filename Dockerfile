FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Lumiere.API/Lumiere.API.csproj", "src/Lumiere.API/"]
COPY ["src/Lumiere.Application/Lumiere.Application.csproj", "src/Lumiere.Application/"]
COPY ["src/Lumiere.Domain/Lumiere.Domain.csproj", "src/Lumiere.Domain/"]
COPY ["src/Lumiere.Infra/Lumiere.Infra.csproj", "src/Lumiere.Infra/"]

RUN dotnet restore "src/Lumiere.API/Lumiere.API.csproj"

COPY . .

WORKDIR "/src/src/Lumiere.API"
RUN dotnet publish "Lumiere.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Lumiere.API.dll"]
