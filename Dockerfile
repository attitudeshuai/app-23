FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/HabitContract.Api/HabitContract.Api.csproj", "src/HabitContract.Api/"]
COPY ["src/HabitContract.Application/HabitContract.Application.csproj", "src/HabitContract.Application/"]
COPY ["src/HabitContract.Domain/HabitContract.Domain.csproj", "src/HabitContract.Domain/"]
COPY ["src/HabitContract.Infrastructure/HabitContract.Infrastructure.csproj", "src/HabitContract.Infrastructure/"]
RUN dotnet restore "src/HabitContract.Api/HabitContract.Api.csproj"
COPY . .
RUN dotnet publish "src/HabitContract.Api/HabitContract.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8093
ENV ASPNETCORE_URLS=http://+:8093
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 CMD curl -f http://localhost:8093/health || exit 1
ENTRYPOINT ["dotnet", "HabitContract.Api.dll"]
