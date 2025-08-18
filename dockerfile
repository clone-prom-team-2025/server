# -------------------------
# Build stage
# -------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore App.Api/App.Api.csproj
RUN dotnet publish App.Api/App.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# -------------------------
# Runtime stage
# -------------------------
FROM nvidia/cuda:12.4.1-runtime-ubuntu22.04 AS runtime

RUN apt-get update && \
    apt-get install -y software-properties-common wget gnupg2 && \
    add-apt-repository ppa:savoury1/ffmpeg5 && \
    apt-get update && \
    apt-get install -y ffmpeg ca-certificates libsm6 libxext6 libxrender-dev && \
    rm -rf /var/lib/apt/lists/*

RUN apt-get update && \
    apt-get install -y dotnet-runtime-9.0 && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "App.Api.dll"]
