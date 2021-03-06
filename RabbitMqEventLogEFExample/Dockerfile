#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["RabbitMqEventLogEFExample/RabbitMqEventLogEFExample.csproj", "RabbitMqEventLogEFExample/"]
COPY ["EventBus.EventLog.EntityFrameworkCore.Extensions.DependencyInjection/EventBus.EventLog.EntityFrameworkCore.Extensions.DependencyInjection.csproj", "EventBus.EventLog.EntityFrameworkCore.Extensions.DependencyInjection/"]
COPY ["EvenBus.EventLog.EntityFrameworkCore/EventBus.EventLog.EntityFrameworkCore.csproj", "EvenBus.EventLog.EntityFrameworkCore/"]
COPY ["EventBus.EventLog/EventBus.EventLog.csproj", "EventBus.EventLog/"]
COPY ["EventBus/EventBus.csproj", "EventBus/"]
COPY ["EventBus.RabbitMQ.Extensions.DependencyInjection/EventBus.RabbitMQ.Extensions.DependencyInjection.csproj", "EventBus.RabbitMQ.Extensions.DependencyInjection/"]
COPY ["EventBus.RabbitMQ/EventBus.RabbitMQ.csproj", "EventBus.RabbitMQ/"]
RUN dotnet restore "RabbitMqEventLogEFExample/RabbitMqEventLogEFExample.csproj"
COPY . .
WORKDIR "/src/RabbitMqEventLogEFExample"
RUN dotnet build "RabbitMqEventLogEFExample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RabbitMqEventLogEFExample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitMqEventLogEFExample.dll"]
