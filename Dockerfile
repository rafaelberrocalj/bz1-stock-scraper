FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ENV PUPPETEER_SKIP_CHROMIUM_DOWNLOAD=true
ENV PUPPETEER_EXECUTABLE_PATH=/usr/bin/chromium

RUN apt-get update \
    && apt-get install -y chromium \
    fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst fonts-freefont-ttf libxss1 \
    --no-install-recommends

WORKDIR /temp

COPY ./src ./

RUN dotnet build
RUN dotnet publish -c Release -o /App

WORKDIR /App
ENTRYPOINT ["dotnet", "bz1-stock-scraper.dll"]
