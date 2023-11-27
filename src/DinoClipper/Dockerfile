FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app
RUN apt-get update \
    && apt-get upgrade \
    && apt-get install ffmpeg python3 python3-pip -y \
    && pip3 install --upgrade yt-dlp

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
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
RUN mkdir "/data"
# Declare volumes
VOLUME ["/data"]
ENTRYPOINT ["dotnet", "DinoClipper.dll"]
