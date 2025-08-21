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

# Ставимо необхідні пакети
RUN apt-get update && \
    apt-get install -y \
        ca-certificates \
        libsm6 \
        libxext6 \
        libxrender-dev \
        wget \
        tar \
        xz-utils \
        && rm -rf /var/lib/apt/lists/*

# Завантажуємо статичний ffmpeg з підтримкою NVENC
RUN wget https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-amd64-static.tar.xz -O /tmp/ffmpeg.tar.xz && \
    tar -xf /tmp/ffmpeg.tar.xz -C /usr/local --strip-components=1 && \
    rm /tmp/ffmpeg.tar.xz

# Ставимо .NET runtime
RUN apt-get update && \
    apt-get install -y dotnet-runtime-9.0 && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "App.Api.dll"]
