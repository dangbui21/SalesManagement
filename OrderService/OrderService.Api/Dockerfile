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

# Copy app đã publish
COPY --from=build /app/publish .

# EXPOSE cổng (dùng đúng cổng theo service)
EXPOSE 80

# Biến môi trường để enable Swagger trong Production (nếu dùng)
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development

# Chạy ứng dụng
ENTRYPOINT ["dotnet", "OrderService.Api.dll"]
