FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DataHarvesterTabletkaBy/DataHarvesterTabletkaBy.csproj", "DataHarvesterTabletkaBy/"]
COPY ["Common/Common.csproj", "Common/"]
COPY ["DataHarvester/DataHarvester.csproj", "DataHarvester/"]

RUN dotnet restore "DataHarvesterTabletkaBy/DataHarvesterTabletkaBy.csproj"


COPY DataHarvesterTabletkaBy/ DataHarvesterTabletkaBy/
COPY Common/ Common/
COPY DataHarvester/ DataHarvester/

WORKDIR "/src/DataHarvesterTabletkaBy"
RUN dotnet build "DataHarvesterTabletkaBy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataHarvesterTabletkaBy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataHarvesterTabletkaBy.dll"]