#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TestProj/TestProj.csproj", "TestProj/"]
RUN dotnet restore "./TestProj/TestProj.csproj"
COPY . .
WORKDIR "/src/TestProj"
RUN dotnet build "./TestProj.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TestProj.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install dependencies
RUN apt-get update && apt-get install -y zip curl
RUN mkdir /otel
RUN curl -L -o /otel/otel-dotnet-install.sh https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/download/v0.7.0/otel-dotnet-auto-install.sh
RUN chmod +x /otel/otel-dotnet-install.sh

ENV OTEL_DOTNET_AUTO_HOME=/otel


RUN /bin/bash /otel/otel-dotnet-install.sh

# Provide necessary permissions for the script to execute
RUN chmod +x /otel/instrument.sh

COPY platform-detection.sh /otel/

# Run the platform detection script
RUN chmod +x /otel/platform-detection.sh && /otel/platform-detection.sh

ENTRYPOINT ["/bin/bash", "-c", "source /otel/instrument.sh && dotnet TestProj.dll"]