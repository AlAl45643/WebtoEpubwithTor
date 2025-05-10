using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System.IO.Compression;
using System.Diagnostics;

namespace tests
{
    [TestFixture]
    public class TestingSeleniumedTorBrowser
    {

        private static FirefoxDriver SetupTorBrowser()
        {

            FirefoxDriverService firefoxDriverService;
            FirefoxDriver firefoxDriver;
            FirefoxOptions firefoxOptions;
            string currentDirectory = Directory.GetCurrentDirectory();
            string profilePath = currentDirectory + "/resources/tor-browser/Browser/TorBrowser/Data/Browser/profile.default/";
            string binaryPath = currentDirectory + "/resources/tor-browser/Browser/firefox";
            string geckoDriverPath = currentDirectory + "/resources/tor-browser/geckodriver";

            firefoxDriverService = FirefoxDriverService.CreateDefaultService(geckoDriverPath);
            firefoxDriverService.FirefoxBinaryPath = binaryPath;
            firefoxDriverService.BrowserCommunicationPort = 2828;

            firefoxOptions = new()
            {
                LogLevel = FirefoxDriverLogLevel.Trace
            };
            firefoxOptions.AddArguments("-profile", profilePath);

            firefoxDriver = new FirefoxDriver(firefoxDriverService, firefoxOptions, TimeSpan.FromSeconds(180));
            return firefoxDriver;
        }


        [Test]
        /// <summary>
        /// Check if https://check.torproject.org/ tells us that we are using Tor Browser.
        /// </summary>
        public void Is_Selenium_using_tor()
        {
            FirefoxDriver firefoxDriver = SetupTorBrowser();
            string checkTorLink = "https://check.torproject.org/";
            string confirmAttribute;

            using (firefoxDriver)
            {
                // wait.Until checks if TOR has established connection
                WebDriverWait wait = new(firefoxDriver, TimeSpan.FromSeconds(180));
                _ = wait.Until(i =>
                {
                    return firefoxDriver.Title == "";
                });

                firefoxDriver.Navigate().GoToUrl(checkTorLink);
                IWebElement confirmElement = firefoxDriver.FindElement(By.TagName("h1"));
                confirmAttribute = confirmElement.GetAttribute("class");
            }

            Assert.That(confirmAttribute, Is.EqualTo("on"));
        }

        [Test]
        /// <summary>
        /// Check if links are being retrieved correctly from selenium. Asserts true if returnedLinks retrieved from stubMainWebpage is equal to mockLinks.
        /// </summary>
        public void Are_links_retrieved_correctly()
        {
            FirefoxDriver firefoxDriver = SetupTorBrowser();
            string currentDirectory = Directory.GetCurrentDirectory();
            string stubMainWebpage = "file://" + Path.Combine(currentDirectory, "resources", "MainWebpageStub.html");
            Regex regex = new("chapter");
            List<string> returnedLinks = [];
            List<string> mockLinks =
            [
               "file://" + Path.Combine(currentDirectory, "resources", "chapter-1.html"),
               "file://" + Path.Combine(currentDirectory, "resources", "chapter-2.html")
            ];


            using (firefoxDriver)
            {
                // wait.Until checks if TOR has established connection
                WebDriverWait wait = new(firefoxDriver, TimeSpan.FromSeconds(180));
                _ = wait.Until(i =>
                {
                    return firefoxDriver.Title == "";
                });
                firefoxDriver.Navigate().GoToUrl(stubMainWebpage);
                var elements = firefoxDriver.FindElements(By.TagName("a"));
                foreach (var element in elements)
                {
                    var href = element.GetAttribute("href");
                    if (href == null)
                    {
                        continue;
                    }

                    if (regex.IsMatch(href))
                    {
                        returnedLinks.Add(href);
                    }

                }
            }

            Assert.That(mockLinks.SequenceEqual(returnedLinks));
        }


        [Test]
        /// <summary>
        /// Check if html retrieved from links in main webpage are retrieved correctly. Asserts true if returnedChapters retrieved from links in stubMainWebpage is equal to mockChapters.
        /// </summary>
        public void Are_links_bodys_retrieved_correctly()
        {
            FirefoxDriver firefoxDriver = SetupTorBrowser();
            string currentDirectory = Directory.GetCurrentDirectory();
            string stubMainWebpage = "file://" + Path.Combine(currentDirectory, "resources", "MainWebpageStub.html");
            Regex regex = new("chapter");
            List<string> returnedLinks = [];
            List<string> mockChapters =
            [
             "This is chapter 1",
             "This is chapter 2"
            ];
            List<string> returnedChapters = [];

            using (firefoxDriver)
            {
                // wait.Until checks if TOR has established connection
                WebDriverWait wait = new(firefoxDriver, TimeSpan.FromSeconds(180));
                _ = wait.Until(i =>
                {
                    return firefoxDriver.Title == "";
                });
                firefoxDriver.Navigate().GoToUrl(stubMainWebpage);
                var elements = firefoxDriver.FindElements(By.TagName("a"));
                foreach (var element in elements)
                {
                    var href = element.GetAttribute("href");
                    if (href == null)
                    {
                        continue;
                    }

                    if (regex.IsMatch(href))
                    {
                        returnedLinks.Add(href);
                    }
                }
                foreach (string link in returnedLinks)
                {
                    firefoxDriver.Navigate().GoToUrl(link);
                    returnedChapters.Add(firefoxDriver.FindElement(By.TagName("body")).Text);
                }

            }

            Assert.That(mockChapters.SequenceEqual(returnedChapters));
        }
        [Test]
        /// <summary>
        /// Check if valid epub is generated from content in chapters.
        /// </summary>
        public void Are_links_bodys_converted_to_EPUB_correctly()
        {
            string currentExecutionPath = Directory.GetCurrentDirectory();
            string pathToEpubRoot = Path.Combine(currentExecutionPath, "resources", "epub");
            string pathToZipEpubAt = Path.Combine(currentExecutionPath, "resources", "epub.epub");
            string pathToEpubContent = Path.Combine(currentExecutionPath, "resources", "epub", "OEBPS");
            string pathToMimetypeFile = Path.Combine(currentExecutionPath, "resources", "mimetype");
            string pathToEpubValidator = Path.Combine(currentExecutionPath, "resources", "epubcheck-5.2.1", "epubcheck.jar");
            Process epubValidatorProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = $"-jar {pathToEpubValidator} {pathToZipEpubAt}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            List<string> stubChapters =
            [
                "This is chapter 1",
                "This is chapter 2"
            ];
            string epubUID = $"wet/{DateTime.Now}";
            string epubValidatorResult = "";

            // create xhtml chapters in pathToEpubContent
            for (int i = 0; i < stubChapters.Count; i++)
            {
                using (StreamWriter chapterFile = new(Path.Combine(pathToEpubContent, $"{i + 1}.xhtml")))
                {
                    chapterFile.Write($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">\n<head>\n    <title>{i + 1}</title>\n</head>\n<body>\n<p>");
                    chapterFile.Write(stubChapters[i]);
                    chapterFile.Write($"</p>\n</body>\n</html>");

                }
            }
            // create content.opf in pathToEpubContent
            using (StreamWriter contentopf = new(Path.Combine(pathToEpubContent, "content.opf")))
            {
                contentopf.Write($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<package xmlns=\"http://www.idpf.org/2007/opf\" unique-identifier=\"BookID\" version=\"2.0\">\n   <metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:opf=\"http://www.idpf.org/2007/opf\">\n        <dc:title>WET</dc:title>\n        <dc:creator opf:role=\"aut\">WET</dc:creator>\n        <dc:language>en-US</dc:language>\n        <dc:rights>Public Domain</dc:rights>\n        <dc:publisher>WET</dc:publisher>\n        <dc:identifier id=\"BookID\" opf:scheme=\"UUID\">{epubUID}</dc:identifier>\n    </metadata>\n    <manifest>\n        <item id=\"ncx\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\" />\n");

                for (int i = 0; i < stubChapters.Count; i++)
                {
                    contentopf.Write($"      <item id=\"page{i + 1}\" href=\"{i + 1}.xhtml\" media-type=\"application/xhtml+xml\" />\n");
                }

                contentopf.Write($"    </manifest>\n    <spine toc=\"ncx\">\n");

                for (int i = 0; i < stubChapters.Count; i++)
                {
                    contentopf.Write($"       <itemref idref=\"page{i + 1}\" />\n");
                }

                contentopf.Write($"    </spine>\n</package>");
            }
            // create toc.ncx in pathToEpubContent
            using (StreamWriter tocncx = new(Path.Combine(pathToEpubContent, "toc.ncx")))
            {
                tocncx.Write($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<ncx xmlns=\"http://www.daisy.org/z3986/2005/ncx/\" version=\"2005-1\">\n\n<head>\n    <meta name=\"dtb:epubUID\" content=\"{epubUID}\"/>    <meta name=\"dtb:depth\" content=\"1\"/>\n    <meta name=\"dtb:totalPageCount\" content=\"0\"/>\n    <meta name=\"dtb:maxPageNumber\" content=\"0\"/>\n</head>\n\n<docTitle>\n    <text>WET</text>\n</docTitle>\n\n<navMap>\n");

                for (int i = 0; i < stubChapters.Count; i++)
                {
                    tocncx.Write($"    <navPoint id=\"page{i + 1}\" playOrder=\"{i + 1}\">\n        <navLabel>\n        <text>Chapter {i + 1}</text>\n    </navLabel>\n    <content src=\"{i + 1}.xhtml\"/>\n</navPoint>\n");
                }

                tocncx.Write("\n\n</navMap>\n</ncx>");
            }
            // an epub file is just a zip with a .epub extension
            using (FileStream zipFile = new FileStream(pathToZipEpubAt, FileMode.Create))
            {
                using (ZipArchive zipArchive = new ZipArchive(zipFile, ZipArchiveMode.Create))
                {
                    // mimetype file must come first and be uncompressed according to epub specifications
                    zipArchive.CreateEntryFromFile(pathToMimetypeFile, Path.Combine("mimetype"), CompressionLevel.NoCompression);
                    RecursiveEntry(zipArchive, pathToEpubRoot, "");
                }
            }

            epubValidatorProcess.Start();
            while (!epubValidatorProcess.StandardOutput.EndOfStream)
            {
                epubValidatorResult += epubValidatorProcess.StandardOutput.ReadLine();
            }
            Assert.That(epubValidatorResult.Contains("0 errors") && epubValidatorResult.Contains("0 fatals"));

        }

        /// <summary>
        /// Add entries in 'directory' to 'archive' as 'pathInZip' recursively if entry is a directory.
        /// </summary>
        private void RecursiveEntry(ZipArchive zipArchive, string directory, string pathInZip)
        {
            var entries = Directory.EnumerateFileSystemEntries(directory);
            foreach (var entry in entries)
            {
                string entryFileName = Regex.Match(entry, "(?!.*[\\/]).*").Value;
                if (File.GetAttributes(entry).HasFlag(FileAttributes.Directory))
                {
                    string newPathInZip = Path.Combine(pathInZip, entryFileName);
                    RecursiveEntry(zipArchive, entry, newPathInZip);
                    continue;
                }
                zipArchive.CreateEntryFromFile(entry, Path.Combine(pathInZip, entryFileName));
            }
            return;
        }

        //
        [TestCase("https://nhvnovels.com/novels/i-became-the-daughter-of-the-western-general-remake-version/")]
        /// <summary>
        /// Check if our request is being blocked by anything. If so, redirect to user control.
        /// </summary>
        public void Check_if_thing_is_blocking_us(string site)
        {
            Func<IWebDriver, bool> consent = NoInformationConsentForm;
            Func<IWebDriver, bool> cloudflare = NoCloudflareCaptcha;
            List<Func<IWebDriver, bool>> conditions = new() { consent, cloudflare };
            WaitIfConditionsTrue(site, conditions);
        }


        /// <summary>
        /// Waits if any of the conditions are true.
        /// </summary>
        public void WaitIfConditionsTrue(string site, List<Func<IWebDriver, bool>> conditions)
        {
            FirefoxDriver firefoxDriver = SetupTorBrowser();

            using (firefoxDriver)
            {
                WebDriverWait wait = new(firefoxDriver, TimeSpan.FromSeconds(180));
                wait.Until(i =>
                {
                    return firefoxDriver.Title == "";
                });
                firefoxDriver.Navigate().GoToUrl(site);
                // wait.Until checks if TOR has established connection
                foreach (Func<IWebDriver, bool> condition in conditions)
                {
                    wait.Until(condition);
                }
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
    }
}
