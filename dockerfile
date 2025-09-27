# ---------- BUILD STAGE ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore App.Api/App.Api.csproj
RUN dotnet publish App.Api/App.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- RUNTIME STAGE ----------
# Базуємо на офіційному образі ASP.NET з CUDA
FROM nvidia/cuda:12.2.2-runtime-ubuntu22.04 AS runtime-base

# Встановлюємо .NET 9 ASP.NET runtime
RUN apt-get update && \
    apt-get install -y wget apt-transport-https && \
    wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y dotnet-runtime-9.0 aspnetcore-runtime-9.0
    
# Встановлюємо бібліотеки для графіки (wkhtmltopdf залежності)
RUN apt-get update && \
    apt-get install -y \
    libxrender1 \
    libxext6 \
    libfontconfig1 \
    libfreetype6 \
    libjpeg-turbo8 \
    libpng16-16 \
    libssl3 \
    xfonts-75dpi \
    xfonts-base \
    fonts-liberation \
    libglib2.0-0 \
    libx11-6 \
    libxcb1 \
    libx11-xcb1 \
    libxcb-render0 \
    libxcb-shm0 \
    libxrender1 \
    libxext6 \
    libxrandr2 \
    libxfixes3 \
    libxi6 \
    libxinerama1 \
    libxss1 \
    libxcursor1

# Встановлюємо FFmpeg з підтримкою NVENC
RUN apt-get install -y ffmpeg && \
    ffmpeg -hwaccels
    
# Встановлюємо wkhtmltopdf
RUN wget https://github.com/wkhtmltopdf/packaging/releases/download/0.12.6.1-2/wkhtmltox_0.12.6.1-2.jammy_amd64.deb && \
    dpkg -i wkhtmltox_0.12.6.1-2.jammy_amd64.deb || apt-get install -f -y && \
    rm wkhtmltox_0.12.6.1-2.jammy_amd64.deb
    
RUN apt-get install -f -y

# Копіюємо зібраний застосунок
WORKDIR /app
COPY --from=build /app/publish .

# Налаштування
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "App.Api.dll"]
