# ============================================
# Stage 1: Build Vue.js frontend
# ============================================
FROM node:24-alpine AS vue-build

WORKDIR /app/vue-app

COPY src/Web/vue-app/package.json src/Web/vue-app/package-lock.json* ./
RUN npm ci

COPY src/Web/vue-app/ ./
RUN npm run build

# ============================================
# Stage 2: Build .NET application
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dotnet-build

WORKDIR /src

COPY Directory.Build.props* ./
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Persistence/Persistence.csproj src/Persistence/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Web/Web.csproj src/Web/

RUN dotnet restore src/Web/Web.csproj

COPY src/ src/

COPY --from=vue-build /app/wwwroot/ src/Web/wwwroot/

RUN dotnet publish src/Web/Web.csproj -c Release -o /app/publish --no-restore

# ============================================
# Stage 3: Final runtime image
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

RUN apk add --no-cache icu-data-full icu-libs krb5-libs

WORKDIR /app

COPY --from=dotnet-build /app/publish .

RUN mkdir -p /app/logs /app/backups /app/wwwroot/uploads && chown -R $APP_UID:$APP_UID /app/logs /app/backups /app/wwwroot/uploads

USER $APP_UID

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Web.dll"]
