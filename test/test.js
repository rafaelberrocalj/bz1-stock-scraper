const puppeteer = require("puppeteer");

(async () => {
  const browser = await puppeteer.launch();
  const page = await browser.newPage();

  await page.goto("https://investidor10.com.br/fiis/bari11/", {
    waitUntil: "networkidle2",
  });
  await page.pdf({ path: "investidor10.pdf", format: "a4" });
  console.log("investidor10 ok");

  await page.goto("https://statusinvest.com.br/fundos-imobiliarios/bari11", {
    waitUntil: "networkidle2",
  });
  await page.pdf({ path: "statusinvest.pdf", format: "a4" });
  console.log("statusinvest ok");

  await browser.close();
})();
