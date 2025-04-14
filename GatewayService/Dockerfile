# SDK image để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ project
COPY . ./

# Restore và build
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy app đã publish (gồm cả ocelot.json nếu đã cấu hình đúng trong .csproj)
COPY --from=build /app/publish .

# EXPOSE cổng (gateway mở cổng 80)
EXPOSE 80

# Cấu hình môi trường
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development

# Chạy ứng dụng
ENTRYPOINT ["dotnet", "gatewayservice.dll"]
