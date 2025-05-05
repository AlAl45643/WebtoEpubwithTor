using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System.IO.Compression;

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
            string profilePath = "/home/work/tor-browser-linux-x86_64-14.0.7/tor-browser/Browser/TorBrowser/Data/Browser/profile.default/";
            string binaryPath = "/home/work/tor-browser-linux-x86_64-14.0.7/tor-browser/Browser/firefox";
            string geckoDriverPath = "/home/work/tor-browser-linux-x86_64-14.0.7/tor-browser/geckodriver-v0.36.0-linux64/geckodriver";

            firefoxDriverService = FirefoxDriverService.CreateDefaultService(geckoDriverPath);
            firefoxDriverService.FirefoxBinaryPath = binaryPath;
            firefoxDriverService.BrowserCommunicationPort = 2828;

            firefoxOptions = new()
            {
                LogLevel = FirefoxDriverLogLevel.Trace
            };
            firefoxOptions.AddArguments("-profile", profilePath);
            firefoxOptions.SetPreference("marionette.debugging.clicktostart", false);

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
            string currentDirectory = Directory.GetCurrentDirectory();
            string epubDirectory = Path.Combine(currentDirectory, "resources", "epub");
            string epubZipDirectory = Path.Combine(currentDirectory, "resources", "epub.zip");
            string epubContentDirectory = Path.Combine(currentDirectory, "resources", "epub", "OEBPS");
            List<string> stubChapters =
            [
                "This is chapter 1",
                "This is chapter 2"
            ];
            string uid = $"wet{DateTime.Now}";

            // create xhtml chapters in epubContentDirectory
            for (int i = 0; i < stubChapters.Count; i++)
            {
                using (StreamWriter chapterFile = new(Path.Combine(epubContentDirectory, $"{i}.xhtml")))
                {
                    chapterFile.Write($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"DTD/xhtml1-transitional.dtd\">\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">\n<head>\n    <title>{i}</title>\n</head>\n<body>\n");
                    chapterFile.Write(stubChapters[i]);
                    chapterFile.Write($"\n</body>\n</html>");

                }
            }

            var files = Directory.EnumerateFiles(epubContentDirectory);

            foreach (var file in files)
            {
                Console.WriteLine(file);
            }

            // create content.opf in epubContentDirectory
            using (StreamWriter contentopf = new(Path.Combine(epubContentDirectory, "content.opf")))
            {
                contentopf.Write($"<?xml version=\"1.0\" encoding=\"UTF-8\"??>\n<package xmlns=\"http://www.idpf.org/2007/opf\" unique-identifier=\"BookID\" version=\"2.0\">\n   <metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:opf=\"http://www.idpf.org/2007/opf\">\n        <dc:title>WET</dc:title>\n        <dc:creator opf:role=\"aut\">Yoda47</dc:creator>\n        <dc:language>en-US</dc:language>\n        <dc:rights>Public Domain</dc:rights>\n        <dc:publisher>WET</dc:publisher>\n        <dc:identifier id=\"BookID\" opf:scheme=\"UUID\">{uid}</dc:identifier>\n    </metadata>\n    <manifest>\n");

                for (int i = 0; i < stubChapters.Count; i++)
                {
                    contentopf.Write($"<item id=\"page{i}\" href=\"{i}.xhtml\" media-type=\"application / xhtml + xml\" />");
                }

                contentopf.Write($"    </manifest>\n    <spine toc=\"ncx\">\n");

                for (int i = 0; i < stubChapters.Count; i++)
                {
                    contentopf.Write($"<itemref idref=\"page{i}\" />");
                }

                contentopf.Write($"    </spine>\n</package>");
            }

            // create toc.ncx
            using (StreamWriter tocncx = new(Path.Combine(epubContentDirectory, "toc.ncx")))
            {
                tocncx.Write($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<ncx xmlns=\"http://www.daisy.org/z3986/2005/ncx/\" version=\"2005-1\">\n\n<head>\n    <meta name=\"dtb:uid\" content=\"{uid}\"/>    <meta name=\"dtb:depth\" content=\"1\"/>\n    <meta name=\"dtb:totalPageCount\" content=\"0\"/>\n    <meta name=\"dtb:maxPageNumber\" content=\"0\"/>\n</head>\n\n<docTitle>\n    <text>WET</text>\n</docTitle>\n\n<navMap>\n");

                for (int i = 0; i < stubChapters.Count; i++)
                {
                    tocncx.Write($"    <navPoint id=\"chapter{i}\" playOrder=\"{i}\">\n        <navLabel>\n        <text>Chapter {i}</text>\n    </navLabel>\n    <content src=\"{i}.xhtml\"/>\n</navPoint>");
                }

                tocncx.Write("\n\n</navMap>\n</ncx>");
            }

            // zip up epub
            ZipFile.CreateFromDirectory(epubDirectory, epubZipDirectory);
        }
    }
}
