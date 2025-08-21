# Runtime stage
FROM nvidia/cuda:12.4.1-runtime-ubuntu22.04 AS runtime

# Встановлюємо пакети
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
    apt-get install -y aspnetcore-runtime-9.0 && \
    rm -rf /var/lib/apt/lists/*


# Додаємо dotnet в PATH
ENV DOTNET_ROOT=/usr/share/dotnet
ENV PATH=$PATH:/usr/share/dotnet

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "App.Api.dll"]
