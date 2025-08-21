FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore App.Api/App.Api.csproj
RUN dotnet publish App.Api/App.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Install dependencies for CUDA and FFmpeg with GPU support
RUN apt-get update && apt-get install -y --no-install-recommends \
    cuda-libraries-11-2 \
    cuda-cudart-11-2 \
    ffmpeg \
    libnvidia-encode-11-2 \
    && rm -rf /var/lib/apt/lists/*

# Install NVIDIA Container Toolkit for GPU support
RUN distribution=$(. /etc/os-release;echo $ID$VERSION_ID) \
    && curl -fsSL https://nvidia.github.io/libnvidia-container/gpgkey | gpg --dearmor -o /usr/share/keyrings/nvidia-container-toolkit-keyring.gpg \
    && curl -s -L https://nvidia.github.io/libnvidia-container/$distribution/libnvidia-container.list | \
        sed 's#deb https://#deb [signed-by=/usr/share/keyrings/nvidia-container-toolkit-keyring.gpg] https://#g' | \
        tee /etc/apt/sources.list.d/nvidia-container-toolkit.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends nvidia-container-toolkit \
    && rm -rf /var/lib/apt/lists/*

# Configure environment for CUDA
ENV NVIDIA_VISIBLE_DEVICES all
ENV NVIDIA_DRIVER_CAPABILITIES compute,video,utility

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "App.Api.dll"]
