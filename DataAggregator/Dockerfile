FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DataAggregator/DataAggregator.csproj", "DataAggregator/"]
COPY ["Common/Common.csproj", "Common/"]

RUN dotnet restore "DataAggregator/DataAggregator.csproj"

COPY DataAggregator/ DataAggregator/
COPY Common/ Common/

WORKDIR "/src/DataAggregator"
RUN dotnet build "DataAggregator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataAggregator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataAggregator.dll"]