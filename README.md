# DotScrapper ✂

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Edge](https://img.shields.io/badge/Microsoft_Edge-0078D7?style=for-the-badge&logo=Microsoft-edge&logoColor=white)
![Visual Studio](https://img.shields.io/badge/Visual_Studio-5C2D91?style=for-the-badge&logo=visual%20studio&logoColor=white)

A simple .NET 6 Scrapper tool, using [Serilog](https://github.com/serilog/serilog), [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp), [Selenium WebDriver](https://www.selenium.dev/documentation/webdriver/).

This project is in development and it's **not stable yet**, if there are people who want to help me, I am fully open 😊

## Usage 🔎

### How to use ? 

```sh
./DotScrapper -out (output) -query (query) -use (Bing, ...)

./DotScrapper -o (output) -q (query) -u (Bing, ...)
```

### Examples ⌨

```sh
# Will scrap 'raccoon' images in 'raccoon\\' from Bing
./DotScrapper -oq raccoon -u Bing
```

### All Arguments

| Argument  | Small argument | Description |
| ------------- | ------------- | --------|
| -out | -o | Change the output directory to use, by default DotScrapper will use 'Out//'.
| -query | -q |  The text to query, It depends on the scrapper you are using too, for example with Bing you can use the advanced search
| -use | -u | The scrapper to use, for example Bing, ...
| -autoclean | -a | Auto close all 'msedgedriver' that have not been closed from previous executions, *it will be removed on future stable versions of this program.*
