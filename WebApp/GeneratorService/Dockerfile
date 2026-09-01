# Development stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dev
WORKDIR /app
EXPOSE 8080
# By default dotnet watch will use the Development environment
ENV ASPNETCORE_ENVIRONMENT=Development
CMD ["dotnet", "watch", "run", "--no-launch-profile", "--urls", "http://0.0.0.0:8080"]

# Build stage for production AOT
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# Install Native AOT dependencies
RUN apt-get update && apt-get install -y --no-install-recommends clang zlib1g-dev
WORKDIR /src
COPY ["GeneratorService.csproj", "./"]
RUN dotnet restore "./GeneratorService.csproj"
COPY . .
RUN dotnet publish "GeneratorService.csproj" -c Release -o /app/publish

# Production stage
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS prod
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
COPY --from=build /app/publish .
ENTRYPOINT ["./GeneratorService"]
