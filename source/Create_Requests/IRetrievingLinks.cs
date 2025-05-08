using System.Text.RegularExpressions;

namespace source.Create_Requests
{
    public interface IRetrievingLinks
    {
        /// <summary>
        /// Retrieve links from a webpage according to regex pattern.
        /// </summary>
        void RetrieveLinks(List<Page> listOfPages, string link, Regex regex);
    }
}
