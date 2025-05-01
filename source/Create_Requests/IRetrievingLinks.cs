using System.Text.RegularExpressions;

namespace source.Create_Requests
{
    public interface IRetrievingLinks
    {
        /// <summary>
        /// Retrieve links from a webpage according to regex pattern.
        /// </summary>
        /// <returns><c>List<Page></c></returns>
        List<Page> RetrieveLinks(string link, Regex regex);
    }
}
