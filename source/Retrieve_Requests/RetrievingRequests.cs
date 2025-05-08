using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using source.Create_Requests;
namespace source.Retrieve_Requests
{
    public class RetrievingRequests : IRetrievingLinks, IRetrievingWebpages
    {

        /// <summary>
        /// Retrieve links from webpage according to regex pattern.
        /// </summary>
        /// <returns><c>Page[]</c></returns>
        public void RetrieveLinks(List<Page> listOfPages, string link, Regex regex)
        {
            SeleniumedTorBrowser seleniumedTorBrowser = new();
            using (seleniumedTorBrowser.FirefoxDriver)
            {
                // wait.Until checks if TOR has left the "Establishing Connection" page.
                WebDriverWait wait = new(seleniumedTorBrowser.FirefoxDriver, TimeSpan.FromSeconds(180));
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
                        Page page = new();
                        page.Hyperlink = href;
                        listOfPages.Add(page);
                    }
                }

            }

        }
        /// <summary>
        /// Retrieve PageContent for each Hyperlink in List<Page>. PageContent retrieved is the visible text in <body> </body>.
        /// </summary>
        public void RetrieveWebpages(List<Page> listOfPages)
        {
            SeleniumedTorBrowser seleniumedTorBrowser = new();
            using (seleniumedTorBrowser.FirefoxDriver)
            {
                // wait.Until checks if TOR has left the "Establishing Connection" page.
                WebDriverWait wait = new(seleniumedTorBrowser.FirefoxDriver, TimeSpan.FromSeconds(180));
                _ = wait.Until(i =>
                {
                    return seleniumedTorBrowser.FirefoxDriver.Title == "";
                });

                for (int i = 0; i < listOfPages.Count; i++)
                {
                    if (listOfPages[i].Hyperlink == null)
                    {
                        continue;
                    }
                    seleniumedTorBrowser.FirefoxDriver.Navigate().GoToUrl(listOfPages[i].Hyperlink);
                    listOfPages[i].PageContent = seleniumedTorBrowser.FirefoxDriver.FindElement(By.TagName("body")).Text;
                }

            }
        }
    }
}
