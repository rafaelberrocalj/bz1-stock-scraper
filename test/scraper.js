const puppeteer = require("puppeteer");
const fs = require("fs");
const path = require("path");

// Configuração dos tickers (substitui o arquivo appsettings.json)
const tickers = {
  FIIs: [
    "BARI11",
    "BCRI11",
    "CYCR11",
    "ICRI11",
    "KNSC11",
    "MFII11",
    "TGAR11",
    "URPR11",
    "VGHF11",
    "VSLH11",
  ],
  FIInfras: ["IFRI11"],
  FIAgros: ["RURA11"],
};

// Classe para construir o scraper
class StockScraperBuilder {
  constructor(ticker, endpoint, waitForSelector) {
    this.ticker = ticker;
    this.endpoint = endpoint;
    this.waitForSelector = waitForSelector;
    this.selectors = [];
  }

  getTicker() {
    return this.ticker;
  }

  getEndpoint() {
    return this.endpoint;
  }

  getWaitForSelector() {
    return this.waitForSelector;
  }

  getSelectors() {
    return this.selectors;
  }

  addSelector(name, ...selectors) {
    this.selectors.push(...selectors.map((selector) => ({ name, selector })));
    return this;
  }
}

// Classe para construir os scrapers específicos
class StatusInvestComBrScraper {
  constructor() {
    this.baseUrl = "https://statusinvest.com.br/";
    this.waitForSelector =
      "#main-header > div.container > div > div:nth-child(1) > h1";
    this.ticker = null;
    this.endpointPath = null;
  }

  withTicker(ticker) {
    this.ticker = ticker;
    return this;
  }

  withFIIs() {
    this.endpointPath = `fundos-imobiliarios/${this.ticker}`;
    return this;
  }

  withFIInfras() {
    this.endpointPath = `fiinfras/${this.ticker}`;
    return this;
  }

  withFIAgros() {
    this.endpointPath = `fiagros/${this.ticker}`;
    return this;
  }

  build() {
    const endpoint = `${this.baseUrl}${this.endpointPath}`;
    const builder = new StockScraperBuilder(
      this.ticker,
      endpoint,
      this.waitForSelector
    );

    builder
      .addSelector(
        "pvp",
        "/html/body/main/div[2]/div[5]/div/div[2]/div/div[1]/strong",
        "/html/body/main/div[2]/div[4]/div/div[2]/div/div[1]/strong"
      )
      .addSelector(
        "dividend",
        "/html/body/main/div[2]/div[8]/div/div[7]/div/div[2]/table/tbody/tr[1]/td[4]",
        "/html/body/main/div[2]/div[7]/div/div[7]/div/div[2]/table/tbody/tr[1]/td[4]",
        "/html/body/main/div[2]/div[4]/div/div[7]/div/div[2]/table/tbody/tr[1]/td[4]"
      );

    return builder;
  }
}

// Função principal
(async () => {
  const browser = await puppeteer.launch({
    headless: false,
    args: [
      "--no-sandbox",
      "--disable-setuid-sandbox",
      "--disable-dev-shm-usage",
      "--disable-gpu",
    ],
  });

  const page = await browser.newPage();
  await page.setViewport({ width: 1280, height: 1024 });
  await page.setUserAgent(
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36"
  );

  // Construir os scrapers
  const scrapers = [
    ...tickers.FIIs.map((ticker) =>
      new StatusInvestComBrScraper().withTicker(ticker).withFIIs().build()
    ),
    ...tickers.FIInfras.map((ticker) =>
      new StatusInvestComBrScraper().withTicker(ticker).withFIInfras().build()
    ),
    ...tickers.FIAgros.map((ticker) =>
      new StatusInvestComBrScraper().withTicker(ticker).withFIAgros().build()
    ),
  ];

  const tickersData = {};

  for (const scraper of scrapers) {
    const { ticker, endpoint, waitForSelector, selectors } = scraper;

    console.log(`\nScraping data for ${ticker} from ${endpoint}`);

    try {
      await page.goto(endpoint, {
        //waitUntil: "networkidle2",
      });
      await page.waitForSelector(waitForSelector);

      const tickerData = {};

      for (const { name, selector } of selectors) {
        console.log("\nfor iteration", ticker, name, selector);
        try {
          let value = await (
            await page.$(`::-p-xpath(${selector})`)
          ).evaluate((node) => node.innerText);
          tickerData[name] = value;
          console.log(`name=${ticker};value=${value}`);
        } catch (er) {
          //tickerData[name] = "-";
          console.log(`name=${ticker};er=${er}`);
        }
      }

      tickersData[ticker] = tickerData;
    } catch (error) {
      console.error(`Failed to scrape data for ${ticker}:`, error.message);
    }
  }

  await browser.close();

  // Salvar os resultados em um arquivo JSON
  const outputPath = path.join(__dirname, "tickersData.json");
  fs.writeFileSync(outputPath, JSON.stringify(tickersData, null, 2));
  console.log(`Scraping completed. Data saved to ${outputPath}`);
})();
