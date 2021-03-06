#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["RabbitExample/RabbitExample.csproj", "RabbitExample/"]
COPY ["EventBus.RabbitMQ.Extensions.DependencyInjection/EventBus.RabbitMQ.Extensions.DependencyInjection.csproj", "EventBus.RabbitMQ.Extensions.DependencyInjection/"]
COPY ["EventBus.RabbitMQ/EventBus.RabbitMQ.csproj", "EventBus.RabbitMQ/"]
COPY ["EventBus/EventBus.csproj", "EventBus/"]
RUN dotnet restore "RabbitExample/RabbitExample.csproj"
COPY . .
WORKDIR "/src/RabbitExample"
RUN dotnet build "RabbitExample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RabbitExample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitExample.dll"]
