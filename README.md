# bz1-stock-scraper

Simple project for scraping stocks information, to import values for Google Sheets

## Why?

Needed to list some information from stocks to create custom data in Google Sheets

## How to update `tickersData.json`

```
cd src
dotnet run
mv .\tickersData.json ..\tickersData.json -force
```

## How to use?

Install [ImportJSON](https://github.com/bradjasper/ImportJSON) in Google Sheets and start with this sample:

```
=ImportJSON("https://raw.githubusercontent.com/rafaelberrocalj/bz1-stock-scraper/refs/heads/main/tickersData.json"; "/BARI11/dividend"; "noHeaders")
```

### Docker

Build and run locally

```
docker build -t bz1-stock-scraper -f Dockerfile .
docker run -it bz1-stock-scraper
```
