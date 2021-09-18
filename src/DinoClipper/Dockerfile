FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["DinoClipper/DinoClipper.csproj", "DinoClipper/"]
COPY nuget.config .
RUN dotnet restore "DinoClipper/DinoClipper.csproj"
COPY . .
WORKDIR "/src/DinoClipper"
RUN dotnet build "DinoClipper.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DinoClipper.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DinoClipper.dll"]
