FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/CrewTasks.Api/CrewTasks.Api.csproj src/CrewTasks.Api/
COPY src/CrewTasks.Notifications/CrewTasks.Notifications.csproj src/CrewTasks.Notifications/
COPY src/CrewTasks.ServiceDefaults/CrewTasks.ServiceDefaults.csproj src/CrewTasks.ServiceDefaults/
COPY src/CrewTasks.Todos/CrewTasks.Todos.csproj src/CrewTasks.Todos/
COPY src/CrewTasks.Todos.Contracts/CrewTasks.Todos.Contracts.csproj src/CrewTasks.Todos.Contracts/
COPY src/CrewTasks.Users/CrewTasks.Users.csproj src/CrewTasks.Users/
COPY src/CrewTasks.Users.Contracts/CrewTasks.Users.Contracts.csproj src/CrewTasks.Users.Contracts/

RUN dotnet restore src/CrewTasks.Api/CrewTasks.Api.csproj

COPY src/ src/

RUN dotnet publish src/CrewTasks.Api/CrewTasks.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS final
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "CrewTasks.Api.dll"]
