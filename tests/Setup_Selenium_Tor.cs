using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace tests
{
    [TestFixture]
    public class SetupSeleniumTor
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

            firefoxOptions = new FirefoxOptions();
            firefoxOptions.LogLevel = FirefoxDriverLogLevel.Trace;
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
                WebDriverWait wait = new WebDriverWait(firefoxDriver, TimeSpan.FromSeconds(180));
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
            string stubMainWebpage = "file:///home/work/WebtoEpubwithTor/tests/MainWebpageStub.html";
            List<string> mockLinks = new List<string>();
            mockLinks.Add("https://www.fakesite.com/series/fake-novel/chapter-1");
            mockLinks.Add("https://www.fakesite.com/series/fake-novel/chapter-2");
            List<string> returnedLinks = new List<string>();
            Regex regex = new Regex("chapter");

            using (firefoxDriver)
            {
                // wait.Until checks if TOR has established connection
                WebDriverWait wait = new WebDriverWait(firefoxDriver, TimeSpan.FromSeconds(180));
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

    }
}
