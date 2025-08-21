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

# Базові пакети + бібліотеки для ffmpeg
RUN apt-get update && \
    apt-get install -y \
        ca-certificates \
        libsm6 \
        libxext6 \
        libxrender-dev \
        wget \
        tar \
        xz-utils \
        curl \
        gnupg \
    && rm -rf /var/lib/apt/lists/*

# Статичний ffmpeg з NVENC
RUN wget https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-amd64-static.tar.xz -O /tmp/ffmpeg.tar.xz && \
    tar -xf /tmp/ffmpeg.tar.xz -C /usr/local --strip-components=1 && \
    rm /tmp/ffmpeg.tar.xz && \
    ln -s /usr/local/ffmpeg /usr/bin/ffmpeg || true

# Встановлюємо ASP.NET Core Runtime 9.0
RUN mkdir -p /etc/apt/keyrings && \
    curl -sSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /etc/apt/keyrings/microsoft.gpg && \
    chmod go+r /etc/apt/keyrings/microsoft.gpg && \
    echo "deb [arch=amd64 signed-by=/etc/apt/keyrings/microsoft.gpg] https://packages.microsoft.com/ubuntu/22.04/prod jammy main" | tee /etc/apt/sources.list.d/microsoft.list && \
    apt-get update && \
    apt-get install -y aspnetcore-runtime-9.0 && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "App.Api.dll"]

