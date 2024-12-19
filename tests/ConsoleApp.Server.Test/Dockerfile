FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["tests/ConsoleApp.Test/ConsoleApp.Test.csproj", "tests/ConsoleApp.Test/"]
RUN dotnet restore "tests/ConsoleApp.Test/ConsoleApp.Test.csproj"
COPY . .
WORKDIR "/src/tests/ConsoleApp.Test"
RUN dotnet build "ConsoleApp.Test.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConsoleApp.Test.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConsoleApp.Test.dll"]
