using System.Text.RegularExpressions;
using OpenQA.Selenium;

namespace source.Create_Requests
{
    public interface IRetrieveAndExport
    {
        /// <summary>
        /// Retrieve links from a webpage according to regex pattern.
        /// </summary>
        void RetrieveLinks(List<Page> listOfPages, string link, Regex regex);
        /// <summary>
        /// Retrieve PageContent for each HyperLink in List<Page>.
        /// </summary>
        /// <returns><c>List<Page></c></returns>
        void RetrieveWebpages(List<Page> listOfPages);
        /// <summary>
        /// Exports HTML for each Hyperlink to an epub at location
        /// </summary>
        void ExportToEpub(List<Page> listOfPages, string exportToLocation);
    }

}
