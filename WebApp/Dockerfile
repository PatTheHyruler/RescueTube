FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["RescueTube/RescueTube.csproj", "RescueTube/"]
RUN dotnet restore "RescueTube/RescueTube.csproj"
COPY . .
WORKDIR "/src/RescueTube"
RUN dotnet build "RescueTube.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RescueTube.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RescueTube.dll"]
