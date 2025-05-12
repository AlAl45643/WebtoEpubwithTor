using System.Text.RegularExpressions;

namespace source.Create_Requests
{
    /// <summary>
    /// Initialize CreatingRequests with tor selenium backend.
    /// </summary>
    public class CreatingRequests(IRetrieveAndExport retrieveAndExport)
    {

        /// <summary>
        /// Backend for retrieving from webpages and exporting to epubs.
        /// </summary>
        private IRetrieveAndExport retrieveAndExport = retrieveAndExport;

        /// <summary>
        /// List of pages.
        /// </summary>
        public List<Page> listOfPages = [];


        /// <summary>
        /// Send request to retrieve links from webpage according to regex pattern.
        /// </summary>
        public void RequestLinks(string link, Regex regex)
        {
            retrieveAndExport.RetrieveLinks(listOfPages, link, regex);
        }

        /// <summary>
        /// Send request to retrieve webpages from links in listOfPages.
        /// </summary>
        public void RequestWebpages()
        {
            retrieveAndExport.RetrieveWebpages(listOfPages);
        }

        /// <summary>
        /// Displays list of page hyperlinks.
        /// </summary>
        public void Print()
        {
            for (int i = 0; i < listOfPages.Count; i++)
            {
                Console.WriteLine(i + ": " + listOfPages[i].Hyperlink);
            }

        }

        /// <summary>
        /// Display list of page hyperlinks from start to end.
        /// </summary>
        public void Print(int start, int end)
        {
            if (start < 0 || end > listOfPages.Count)
            {
                return;
            }

            for (int i = start; i < end; i++)
            {
                Console.WriteLine(i + ": " + listOfPages[i].Hyperlink);
            }

        }

        /// <summary>
        /// Remove page at index.
        /// </summary>
        public void Remove(int index)
        {
            listOfPages.RemoveAt(index);
        }

        /// <summary>
        /// Add page at index.
        /// </summary>
        public void Add(int index, string link)
        {
            Page page = new()
            {
                Hyperlink = link
            };
            listOfPages.Insert(index, page);
        }

        /// <summary>
        /// Reverse list of pages so that index 0 becomes index n and index n becomes index 0.
        /// </summary>
        public void Reverse()
        {
            listOfPages.Reverse();
        }

        /// <summary>
        /// Assemble epub from each Page.HTML in List<Page> to exportToLocation.
        /// </summary>
        public void ExportToEpub(string exportToLocation)
        {
            retrieveAndExport.ExportToEpub(listOfPages, exportToLocation);
        }

    }
}
