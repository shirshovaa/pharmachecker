FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM rabbitmq:3.2-management
RUN apt-get update && apt-get install -y curl
RUN curl http://www.rabbitmq.com/community-plugins/v3.6.x/rabbitmq_delayed_message_exchange-0.0.1.ez > $RABBITMQ_HOME/plugins/rabbitmq_delayed_message_exchange-0.0.1.ez
RUN rabbitmq-plugins enable --offline rabbitmq_delayed_message_exchange

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Orchestrator/Orchestrator.csproj", "Orchestrator/"]
COPY ["Common/Common.csproj", "Common/"]

RUN dotnet restore "Orchestrator/Orchestrator.csproj"


COPY Orchestrator/ Orchestrator/
COPY Common/ Common/

WORKDIR "/src/Orchestrator"
RUN dotnet build "Orchestrator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orchestrator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orchestrator.dll"]