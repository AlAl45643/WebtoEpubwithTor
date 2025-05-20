using System.Text.RegularExpressions;
using System.Diagnostics;
using source.Create_Requests;

namespace tests
{
    [TestFixture]
    public class TestingSeleniumedTorBrowser
    {
        [Test]
        /// <summary>
        /// Checks if CreatingRequests.RequestLinks retrieves links correctly.
        /// </summary>
        public void Are_links_retrieved_correctly()
        {
            CreatingRequests creatingRequests = new("request");

            string currentWorkingDirectory = Directory.GetCurrentDirectory();
            string mainWebpageStub = "file://" + Path.Combine(currentWorkingDirectory, "resources", "MainWebpageStub.html");
            Regex regex = new("chapter");
            creatingRequests.RequestLinks(mainWebpageStub, regex);

            string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string tempRequestPath = Path.Combine(homeDirectory, ".wet", "request");
            string[] tempRequestActualResult = File.ReadAllLines(tempRequestPath);
            string[] tempRequestExpectedResult = [$"file://{currentWorkingDirectory}/resources/chapter-1.html", $"file://{currentWorkingDirectory}/resources/chapter-2.html"];
            Assert.That(tempRequestActualResult.SequenceEqual(tempRequestExpectedResult));
        }

        [Test]
        /// <summary>
        /// Checks if CreatingRequests.ExportToEpub produces a valid epub with epubcheck.jar
        /// </summary>
        public void Check_if_epub_is_valid()
        {
            string currentWorkingDirectory = Directory.GetCurrentDirectory();
            string mainWebpageStub = "file://" + Path.Combine(currentWorkingDirectory, "resources", "MainWebpageStub.html");
            Regex regex = new("chapter");
            CreatingRequests creatingRequests = new("request");
            creatingRequests.RequestLinks(mainWebpageStub, regex);

            string epubPath = Path.Combine("resources", "test.epub");
            creatingRequests.ExportToEpub(epubPath);

            string pathToEpubValidator = Path.Combine(currentWorkingDirectory, "resources", "epubcheck-5.2.1", "epubcheck.jar");
            Process epubValidatorProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = $"-jar {pathToEpubValidator} {epubPath}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            string epubValidatorResult = "";
            epubValidatorProcess.Start();
            while (!epubValidatorProcess.StandardOutput.EndOfStream)
            {
                epubValidatorResult += epubValidatorProcess.StandardOutput.ReadLine();
            }
            Assert.That(epubValidatorResult.Contains("0 errors") && epubValidatorResult.Contains("0 fatals"));
        }

        // cloudflare captcha test case
        [TestCase("https://2captcha.com/demo/cloudflare-turnstile-challenge")]
        /// <summary>
        /// Check if CreatingRequests.RetrieveLinks waits for user to bypass blockers such as captchas or consent forms.
        /// </summary>
        public void Does_RetrieveLinks_wait_for_blockers(string site)
        {
            CreatingRequests creatingRequest = new("temp");

            Regex regex = new("");
            creatingRequest.RequestLinks(site, regex);

            Assert.That(true);
        }

        [Test]
        /// <summary>
        /// Checks if typical use of the WET runs without encountering any errors.
        /// </summary>
        public void Does_WET_run_correctly()
        {
            CreatingRequests creatingRequests = new("temp");

            Regex regex = new("chapter");
            creatingRequests.RequestLinks("https://hiraethtranslation.com/novel/the-speedrun-manual-of-miss-witch/", regex);
            creatingRequests.Print();
            creatingRequests.RemoveAt(212);
            creatingRequests.RemoveAt(213);
            creatingRequests.Reverse();
            creatingRequests.Print();
            creatingRequests.ExportToEpub("./epub.epub");

            Assert.That(true);

        }

    }
}
