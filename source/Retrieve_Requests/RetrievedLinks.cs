using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace source.Retrieve_Requests
{
    public class RetrievedLinks
    {
        /// <summary>
        /// Set up Selenium with Tor Browser.
        /// </summary>
        private readonly SeleniumedTorBrowser seleniumedTorBrowser;

        /// <summary>
        /// Constructs RetrievedLinks.
        /// </summary>
        public RetrievedLinks()
        {
            seleniumedTorBrowser = new SeleniumedTorBrowser();

        }

        /// <summary>
        /// Retrieve links from webpage according to regex pattern.
        /// </summary>
        /// <returns><c>Page[]</c></returns>
        public List<source.Page> RetrieveLinks(string link, Regex regex)
        {
            List<source.Page> listOfPages = new List<source.Page>();

            using (seleniumedTorBrowser.FirefoxDriver)
            {
                // wait.Until checks if TOR has left the "Establishing Connection" page.
                WebDriverWait wait = new WebDriverWait(seleniumedTorBrowser.FirefoxDriver, TimeSpan.FromSeconds(180));
                _ = wait.Until(i =>
                {
                    return seleniumedTorBrowser.FirefoxDriver.Title == "";
                });

                seleniumedTorBrowser.FirefoxDriver.Navigate().GoToUrl(link);
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
                        source.Page page = new source.Page();
                        page.Hyperlink = href;
                        listOfPages.Add(page);
                    }
                }

                return listOfPages;
            }
        }
    }
}
