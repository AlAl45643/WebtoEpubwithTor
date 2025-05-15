using System.IO.Compression;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
namespace source.Retrieve_Requests
{
    public class RetrievingRequests
    {

        /// <summary>
        /// Retrieve links from webpage according to regex pattern.
        /// </summary>
        /// <returns><c>Page[]</c></returns>
        public void RetrieveLinks(string requestFilePath, string link, Regex regex)
        {
            SeleniumedTorBrowser seleniumedTorBrowser = new();
            StreamWriter requestFile = File.AppendText(requestFilePath);
            using (seleniumedTorBrowser.FirefoxDriver)
            {
                WaitForTorConnection(seleniumedTorBrowser);
                seleniumedTorBrowser.FirefoxDriver.Navigate().GoToUrl(link);
                WaitForBlockers(seleniumedTorBrowser);
                var elements = seleniumedTorBrowser.FirefoxDriver.FindElements(By.TagName("a"));
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
        /// Retrieve PageContent for each Hyperlink in List<Page>. PageContent retrieved is the visible text in <body> </body>.
        /// </summary>
        public void ExportToEpub(string requestFilePath, string exportToPath)
        {
            SeleniumedTorBrowser seleniumedTorBrowser = new();
            List<string> listOfPages = new();
            using (seleniumedTorBrowser.FirefoxDriver)
            {
                WaitForTorConnection(seleniumedTorBrowser);
                IEnumerable<string> lines = File.ReadLines(requestFilePath);
                foreach (string line in lines)
                {
                    if (line == "")
                    {
                        continue;
                    }
                    seleniumedTorBrowser.FirefoxDriver.Navigate().GoToUrl(line);
                    WaitForBlockers(seleniumedTorBrowser);
                    listOfPages.Add(seleniumedTorBrowser.FirefoxDriver.FindElement(By.TagName("body")).Text);
                }
            }
            File.Delete(requestFilePath);
            ExportToEpub(listOfPages, exportToPath);
        }

        /// <summary>
        /// Check if our request has been blocked by anything. If so, wait for user input.
        /// </summary>
        public void WaitForBlockers(SeleniumedTorBrowser seleniumedTorBrowser)
        {
            Func<IWebDriver, bool> consent = NoInformationConsentForm;
            Func<IWebDriver, bool> cloudflare = NoCloudflareCaptcha;
            List<Func<IWebDriver, bool>> conditions = new() { consent, cloudflare };
            WebDriverWait wait = new(seleniumedTorBrowser.FirefoxDriver, TimeSpan.FromSeconds(180));
            foreach (Func<IWebDriver, bool> condition in conditions)
            {
                wait.Until(condition);
            }
        }

        /// <summary>
        /// Wait until tor browser has established a connection.
        /// </summary>
        private void WaitForTorConnection(SeleniumedTorBrowser seleniumedTorBrowser)
        {
            WebDriverWait wait = new(seleniumedTorBrowser.FirefoxDriver, TimeSpan.FromSeconds(180));
            _ = wait.Until(i =>
            {
                return seleniumedTorBrowser.FirefoxDriver.Title == "";
            });
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
        /// Exports an epub by first converting HTML to XHTML, second creating content.opf and toc.ncx according to epub specification, and finally zipping up the arranged folder with a .epub extension. Epub is created in exportToPath from each Page.HTML in listOfPages.
        /// </summary>
        private void ExportToEpub(List<string> pages, string exportToPath)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string pathToEpubContent = Path.Combine(currentDirectory, "resources", "epub", "OEBPS");
            string epubUID = $"WET/{DateTime.Now}";

            // create xhtml chapters in pathToEpubContent
            for (int i = 0; i < pages.Count; i++)
            {
                string chapterFilePath = Path.Combine(pathToEpubContent, $"{i + 1}.xhtml");
                File.WriteAllText(chapterFilePath, $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">\n<head>\n    <title>{i + 1}</title>\n</head>\n<body>\n<p>{pages[i]}</p>\n</body>\n</html>");
            }

            // create content.opf in pathToEpubContent
            string contentopfPath = Path.Combine(pathToEpubContent, "content.opf");
            using (StreamWriter contentopfStream = new(contentopfPath))
            {
                contentopfStream.Write($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<package xmlns=\"http://www.idpf.org/2007/opf\" unique-identifier=\"BookID\" version=\"2.0\">\n   <metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:opf=\"http://www.idpf.org/2007/opf\">\n        <dc:title>WET</dc:title>\n        <dc:creator opf:role=\"aut\">WET</dc:creator>\n        <dc:language>en-US</dc:language>\n        <dc:rights>Public Domain</dc:rights>\n        <dc:publisher>WET</dc:publisher>\n        <dc:identifier id=\"BookID\" opf:scheme=\"UUID\">{epubUID}</dc:identifier>\n    </metadata>\n    <manifest>\n        <item id=\"ncx\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\" />\n");

                for (int i = 0; i < pages.Count; i++)
                {
                    contentopfStream.Write($"      <item id=\"page{i + 1}\" href=\"{i + 1}.xhtml\" media-type=\"application/xhtml+xml\" />\n");
                }

                contentopfStream.Write($"    </manifest>\n    <spine toc=\"ncx\">\n");

                for (int i = 0; i < pages.Count; i++)
                {
                    contentopfStream.Write($"       <itemref idref=\"page{i + 1}\" />\n");
                }

                contentopfStream.Write($"    </spine>\n</package>");
            }

            // creat toc.ncx in pathToEpubContent
            string tocncx = Path.Combine(pathToEpubContent, "toc.ncx");
            using (StreamWriter tocncxStream = new(tocncx))
            {
                tocncxStream.Write($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<ncx xmlns=\"http://www.daisy.org/z3986/2005/ncx/\" version=\"2005-1\">\n\n<head>\n    <meta name=\"dtb:epubUID\" content=\"{epubUID}\"/>    <meta name=\"dtb:depth\" content=\"1\"/>\n    <meta name=\"dtb:totalPageCount\" content=\"0\"/>\n    <meta name=\"dtb:maxPageNumber\" content=\"0\"/>\n</head>\n\n<docTitle>\n    <text>WET</text>\n</docTitle>\n\n<navMap>\n");

                for (int i = 0; i < pages.Count; i++)
                {
                    tocncxStream.Write($"    <navPoint id=\"page{i + 1}\" playOrder=\"{i + 1}\">\n        <navLabel>\n        <text>Chapter {i + 1}</text>\n    </navLabel>\n    <content src=\"{i + 1}.xhtml\"/>\n</navPoint>\n");
                }

                tocncxStream.Write("\n\n</navMap>\n</ncx>");
            }

            // an epub file is just a zip with a .epub extension
            string pathToEpubRoot = Path.Combine(currentDirectory, "resources", "epub");
            string pathToMimetypeFile = Path.Combine(currentDirectory, "resources", "mimetype");
            using (FileStream zipFile = new FileStream(exportToPath, FileMode.Create))
            {
                using (ZipArchive zipArchive = new ZipArchive(zipFile, ZipArchiveMode.Create))
                {
                    // mimetype file must come first and be uncompressed according to epub specifications
                    zipArchive.CreateEntryFromFile(pathToMimetypeFile, "mimetype" , CompressionLevel.NoCompression);
                    RecursiveEntry(zipArchive, pathToEpubRoot, "");
                }
            }
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
    }
}
