FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["RescueTube.WebApi/RescueTube.WebApi.csproj", "RescueTube.WebApi/"]
RUN dotnet restore "RescueTube.WebApi/RescueTube.WebApi.csproj"
COPY . .
WORKDIR "/src/RescueTube.WebApi"
RUN dotnet build "RescueTube.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RescueTube.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN apt update
RUN apt install -y python3
ENTRYPOINT ["dotnet", "RescueTube.WebApi.dll"]
