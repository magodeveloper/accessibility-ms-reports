# ============ STAGE 1: build ============
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Reports.sln ./
COPY Directory.Packages.props ./
COPY src/Reports.Api/Reports.Api.csproj                      src/Reports.Api/
COPY src/Reports.Application/Reports.Application.csproj      src/Reports.Application/
COPY src/Reports.Domain/Reports.Domain.csproj                src/Reports.Domain/
COPY src/Reports.Infrastructure/Reports.Infrastructure.csproj src/Reports.Infrastructure/
COPY src/Reports.Tests/Reports.Tests.csproj                  src/Reports.Tests/

RUN dotnet restore Reports.sln

COPY . .
RUN dotnet publish ./src/Reports.Api/Reports.Api.csproj -c Release -o /app/publish

# ============ STAGE 2: runtime ============
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 8083
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Reports.Api.dll", "--urls", "http://0.0.0.0:8083"]