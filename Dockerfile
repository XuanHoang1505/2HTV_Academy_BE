FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 10000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj và clean trước
COPY ["*.csproj", "./"]

# Xóa cache NuGet và restore với options đặc biệt
RUN dotnet nuget locals all --clear && \
    dotnet restore --disable-parallel --no-cache

# Copy toàn bộ source code
COPY . .

# Build project với cấu hình bỏ qua warning
RUN dotnet build -c Release -o /app/build \
    /p:TreatWarningsAsErrors=false \
    /p:WarningLevel=0

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish \
    /p:UseAppHost=false \
    /p:TreatWarningsAsErrors=false

FROM base AS final
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

# Healthcheck
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:10000/health || curl -f http://localhost:10000/api/health || exit 1

# Run application directly
ENTRYPOINT ["dotnet", "be.dll"]