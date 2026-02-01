FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 10000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj và restore dependencies
COPY ["*.csproj", "./"]
RUN dotnet restore

# Copy toàn bộ source code
COPY . .

# Build project
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

# Create startup script
RUN echo '#!/bin/bash\n\
echo "Starting .NET application..."\n\
dotnet *.dll' > /app/start.sh && chmod +x /app/start.sh

# Healthcheck
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:10000/health || curl -f http://localhost:10000/api/health || exit 1

ENTRYPOINT ["/app/start.sh"]
```

**Lưu ý quan trọng khi deploy lên Render:**

1. **Tạo file `.dockerignore`** để giảm kích thước build:
```
bin/
obj/
.vs/
.vscode/
*.user
*.suo
.git/
.gitignore
README.md
.env
appsettings.Development.json