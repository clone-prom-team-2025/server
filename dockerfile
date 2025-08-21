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

# Встановлюємо пакети для ffmpeg та завантаження
RUN apt-get update && \
    apt-get install -y \
        ca-certificates \
        libsm6 \
        libxext6 \
        libxrender-dev \
        wget \
        tar \
        xz-utils \
        apt-transport-https \
        gnupg \
    && rm -rf /var/lib/apt/lists/*

# Статичний ffmpeg з NVENC
RUN wget https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-amd64-static.tar.xz -O /tmp/ffmpeg.tar.xz && \
    tar -xf /tmp/ffmpeg.tar.xz -C /usr/local --strip-components=1 && \
    rm /tmp/ffmpeg.tar.xz

# Підключаємо Microsoft репозиторій та ставимо .NET 9 runtime
RUN wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y dotnet-runtime-9.0 && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "App.Api.dll"]
