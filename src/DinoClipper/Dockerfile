FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app
RUN apt-get update && apt-get upgrade -y
RUN apt-get install ffmpeg python3 python3-pip -y
RUN rm /usr/lib/python3.*/EXTERNALLY-MANAGED
RUN pip3 install --upgrade yt-dlp

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /proj
COPY . .
RUN dotnet restore "src/DinoClipper/DinoClipper.csproj"
WORKDIR "/proj/src/DinoClipper"
RUN dotnet build "DinoClipper.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/proj/src/DinoClipper"
RUN dotnet publish "DinoClipper.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir "/data"
# Declare volumes
VOLUME ["/data"]
CMD ["dotnet", "DinoClipper.dll"]
