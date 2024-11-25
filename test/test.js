const puppeteer = require("puppeteer");

(async () => {
  const browser = await puppeteer.launch();
  const page = await browser.newPage();
  await page.goto("https://news.ycombinator.com", {
    waitUntil: "networkidle2",
  });
  await page.pdf({ path: "test.pdf", format: "a4" });

  var text = await page.$x(
    "/html/body/center/table/tbody/tr[3]/td/table/tbody/tr[1]"
  );

  console.log("text is:", text);

  await browser.close();
})();
