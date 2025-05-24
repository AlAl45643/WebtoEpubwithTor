using System.IO.Compression;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
namespace source.Retrieve_Requests
{
    public class RetrievingRequests
    {

        /// <summary>
        /// Exposes the service provided by the tor browser.
        /// </summary>
        private readonly FirefoxDriverService firefoxDriverService;

        /// <summary>
        /// The options to set for tor browser.
        /// </summary>
        private readonly FirefoxOptions firefoxOptions;

        /// <summary>
        /// Constructor to set log level for tor browser.
        /// </summary>
        public RetrievingRequests(bool trace)
        {

            string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string torDirectory = Path.Combine(homeDirectory, ".wet", "tor-browser");
            string profilePath = Path.Combine([torDirectory, "Browser", "TorBrowser", "Data", "Browser", "profile.default"]);
            string binaryPath = Path.Combine(torDirectory, "Browser", "firefox");
            string geckoDriverPath = Path.Combine(torDirectory);

            firefoxDriverService = FirefoxDriverService.CreateDefaultService(geckoDriverPath);
            firefoxDriverService.FirefoxBinaryPath = binaryPath;
            firefoxDriverService.BrowserCommunicationPort = 2828;

            firefoxOptions = new();
            if (trace)
            {
                firefoxOptions.LogLevel = FirefoxDriverLogLevel.Trace;
            }
            else
            {
                firefoxOptions.LogLevel = FirefoxDriverLogLevel.Fatal;
            }
            firefoxOptions.AddArguments("-profile", profilePath);
            firefoxOptions.SetPreference("marionette.debugging.clicktostart", false);
            firefoxOptions.SetPreference("torbrowser.settings.quickstart.enabled", true);

        }


        /// <summary>
        /// Retrieve links from webpage according to regex pattern.
        /// </summary>
        /// <returns><c>Page[]</c></returns>
        public void RetrieveLinks(string requestFilePath, string link, Regex regex)
        {
            FirefoxDriver torBrowser = new(firefoxDriverService, firefoxOptions, TimeSpan.FromSeconds(180));
            StreamWriter requestFile = File.AppendText(requestFilePath);
            using (torBrowser)
            {
                WaitForTorConnection(torBrowser);
                torBrowser.Navigate().GoToUrl(link);
                WaitForBlockers(torBrowser);
                var elements = torBrowser.FindElements(By.TagName("a"));
                foreach (var element in elements)
                {
                    var href = element.GetAttribute("href");
                    if (href == null)
                    {
                        continue;
                    }

                    if (regex.IsMatch(href))
                    {
                        requestFile.WriteLine($"{href}");
                    }
                }
            }
            requestFile.Close();
        }

        /// <summary>
        /// Export epub from each link in <paramref name="requestFilePath"/> at <paramref name="exportToPath"/>.
        /// </summary>
        public void ExportToEpub(string requestFilePath, string exportToPath)
        {
            FirefoxDriver torBrowser = new(firefoxDriverService, firefoxOptions, TimeSpan.FromSeconds(180));
            List<string> listOfPages = [];
            using (torBrowser)
            {
                WaitForTorConnection(torBrowser);
                IEnumerable<string> lines = File.ReadLines(requestFilePath);
                foreach (string line in lines)
                {
                    if (line == "")
                    {
                        continue;
                    }
                    torBrowser.Navigate().GoToUrl(line);
                    WaitForBlockers(torBrowser);
                    listOfPages.Add(torBrowser.FindElement(By.TagName("body")).Text);
                }
            }
            File.Delete(requestFilePath);
            ExportToEpub(listOfPages, exportToPath);
        }

        /// <summary>
        /// Wait until tor browser has established a connection.
        /// </summary>
        private static void WaitForTorConnection(FirefoxDriver driver)
        {
            WebDriverWait wait = new(driver, TimeSpan.FromSeconds(180));
            _ = wait.Until(i =>
            {
                return driver.Title == "";
            });
        }

        /// <summary>
        /// Check if our request has been blocked by anything. If so, wait for user input.
        /// </summary>
        public void WaitForBlockers(FirefoxDriver driver)
        {
            List<Func<IWebDriver, bool>> conditions = new() { NoInformationConsentForm, NoCloudflareCaptcha, NoCloudflareCaptcha2 };
            WebDriverWait wait = new(driver, TimeSpan.FromSeconds(180));
            foreach (Func<IWebDriver, bool> condition in conditions)
            {
                wait.Until(condition);
            }
        }


        /// <summary>
        /// Returns false if Information Consent Form is visible.
        /// </summary>
        /// <returns><c>bool</c></returns>
        private bool NoInformationConsentForm(IWebDriver driver)
        {
            return driver.FindElements(By.ClassName("fc-consent-root")).Count == 0;
        }

        /// <summary>
        /// Returns false if Cloudflare Captcha is visible.
        /// </summary>
        /// <returns><c>bool</c></returns>
        private bool NoCloudflareCaptcha(IWebDriver driver)
        {
            return driver.FindElements(By.Id("chk-hdr")).Count == 0;
        }

        /// <summary>
        /// Returns false if Cloudflare Captcha is visible
        /// </summary>
        /// <returns><c>bool</c></returns>
        private bool NoCloudflareCaptcha2(IWebDriver driver)
        {
            return driver.FindElements(By.Id("JdYY6")).Count == 0;
        }

        /// <summary>
        /// Exports an epub by first converting webpage HTML to XHTML for each chapter, second creating content.opf and toc.ncx according to epub specification, and finally zipping up the arranged folder with a .epub extension. Epub is created in <paramref name="exportToPath"/> from each page in <paramref name="pages"/>
        /// </summary>
        private static void ExportToEpub(List<string> pages, string exportToPath)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string pathToEpubContent = Path.Combine(currentDirectory, "resources", "epub", "OEBPS");
            string epubUID = $"WET/{DateTime.Now}";

            WriteEpubChapters(pages, pathToEpubContent);

            string contentopfPath = Path.Combine(pathToEpubContent, "content.opf");
            WriteContentOPF(pages, epubUID, contentopfPath);

            string tocncxPath = Path.Combine(pathToEpubContent, "toc.ncx");
            WriteToxNCX(pages, epubUID, tocncxPath);

            string pathToEpubRoot = Path.Combine(currentDirectory, "resources", "epub");
            CreateEpub(exportToPath, currentDirectory, pathToEpubRoot);
        }


        /// <summary>
        /// Write toc.ncx at <paramref name="tocnxPath"/> referencing each page in <paramref name="pages"/> with <paramref name="epubUID"/>
        /// </summary>
        private static void WriteToxNCX(List<string> pages, string epubUID, string tocnxPath)
        {
            using StreamWriter tocncxStream = new(tocnxPath);
            tocncxStream.Write($"""
                                        <?xml version="1.0" encoding="UTF-8"?>
                                        <ncx xmlns="http://www.daisy.org/z3986/2005/ncx/" version="2005-1">

                                        <head>
                                            <meta name="dtb:epubUID" content="{epubUID}"/>
                                            <meta name="dtb:depth" content="1"/>
                                            <meta name="dtb:totalPageCount" content="0"/>
                                            <meta name="dtb:maxPageNumber" content="0"/>
                                        </head>

                                        <docTitle>
                                            <text>WET</text>
                                        </docTitle>

                                        <navMap>
                                        """);

            for (int i = 0; i < pages.Count; i++)
            {
                tocncxStream.Write($"""
                                            <navPoint id="page{i + 1}" playOrder="{i + 1}">
                                                <navLabel>
                                                <text>Chapter {i + 1}</text>
                                            </navLabel>
                                            <content src="{i + 1}.xhtml"/>
                                        </navPoint>
                                        """);
            }

            tocncxStream.Write($"""
                                        </navMap>
                                        </ncx>
                                        """);
        }

        /// <summary>
        /// Write i.xhtml for each pages at <paramref name="pathToEpubContent"/>.
        /// </summary>
        private static void WriteEpubChapters(List<string> pages, string pathToEpubContent)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                string chapterFilePath = Path.Combine(pathToEpubContent, $"{i + 1}.xhtml");
                File.Create(chapterFilePath).Close();
                using StreamWriter chapterFileStream = File.AppendText(chapterFilePath);
                chapterFileStream.Write($"""
                                                   <?xml version="1.0" encoding="UTF-8"?>
                                                   <!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
                                                   <html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
                                                   <head>
                                                       <title>{i + 1}</title>
                                                   </head>
                                                       <body>
                                                   """);

                string[] chapterLines = pages[i].Split(
                    [Environment.NewLine],
                    StringSplitOptions.None
                );
                foreach (string line in chapterLines)
                {
                    chapterFileStream.WriteLine($"""
                                                       <p>{line}</p>
                                                   """);
                }

                chapterFileStream.Write($"""
                                                       </body>
                                                   </html>
                                                   """);
            }
        }

        /// <summary>
        /// Create content.opf at <paramref name="contentopfPath"/> that references each page in <paramref name="pages"/> with <paramref name="epubUID"/>.
        /// </summary>
        private static void WriteContentOPF(List<string> pages, string epubUID, string contentopfPath)
        {
            using StreamWriter contentopfStream = new(contentopfPath);
            contentopfStream.Write($"""
                                        <?xml version="1.0" encoding="UTF-8"?>
                                        <package xmlns="http://www.idpf.org/2007/opf" unique-identifier="BookID" version="2.0">
                                           <metadata xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:opf="http://www.idpf.org/2007/opf">
                                                <dc:title>WET</dc:title>
                                                <dc:creator opf:role="aut">WET</dc:creator>
                                                <dc:language>en-US</dc:language>
                                                <dc:rights>Public Domain</dc:rights>
                                                <dc:publisher>WET</dc:publisher>
                                                <dc:identifier id="BookID" opf:scheme="UUID">{epubUID}</dc:identifier>
                                            </metadata>
                                            <manifest>
                                              <item id="ncx" href="toc.ncx" media-type="application/x-dtbncx+xml" />
                                        """);

            for (int i = 0; i < pages.Count; i++)
            {
                contentopfStream.Write($"""
                                              <item id="page{i + 1}" href="{i + 1}.xhtml" media-type="application/xhtml+xml" />
                                        """);
            }

            contentopfStream.Write($"""
                                            </manifest>
                                            <spine toc="ncx">
                                        """);

            for (int i = 0; i < pages.Count; i++)
            {
                contentopfStream.Write($"""
                                               <itemref idref="page{i + 1}" />
                                        """);
            }

            contentopfStream.Write($"""
                                            </spine>
                                        </package>
                                        """);
        }

        /// <summary>
        /// Create epub at <paramref name="exportToPath"/> by zipping the <paramref name="pathToEpubRoot"/> at <paramref name="currentDirectory"/> according to epub specification.
        /// </summary>
        private static void CreateEpub(string exportToPath, string currentDirectory, string pathToEpubRoot)
        {
            try
            {
                using FileStream zipFile = new(exportToPath, FileMode.Create);
                using ZipArchive zipArchive = new(zipFile, ZipArchiveMode.Create);
                string pathToMimetypeFile = Path.Combine(currentDirectory, "resources", "mimetype");
                // mimetype file must come first and be uncompressed according to epub specifications
                _ = zipArchive.CreateEntryFromFile(pathToMimetypeFile, "mimetype", CompressionLevel.NoCompression);
                RecursiveEntry(zipArchive, pathToEpubRoot, "");
            }
            finally
            {
                Directory.Delete(pathToEpubRoot, true);
            }
        }




        /// <summary>
        /// Add entries in <paramref name="directory"/> to <paramref name="zipArchive"/> as <paramref name="pathInZip"/> recursively if entry is a directory.
        /// </summary>
        private static void RecursiveEntry(ZipArchive zipArchive, string directory, string pathInZip)
        {
            IEnumerable<string> entries = Directory.EnumerateFileSystemEntries(directory);
            foreach (string entry in entries)
            {
                string entryFileName = Regex.Match(entry, "(?!.*[\\/]).*").Value;
                if (File.GetAttributes(entry).HasFlag(FileAttributes.Directory))
                {
                    string newPathInZip = Path.Combine(pathInZip, entryFileName);
                    RecursiveEntry(zipArchive, entry, newPathInZip);
                    continue;
                }
                _ = zipArchive.CreateEntryFromFile(entry, Path.Combine(pathInZip, entryFileName));
            }
            return;
        }
    }
}
