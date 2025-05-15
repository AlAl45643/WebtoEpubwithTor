using System.Text.RegularExpressions;
using source.Retrieve_Requests;

namespace source.Create_Requests
{
    /// <summary>
    /// Initialize CreatingRequests with tor selenium backend.
    /// </summary>
    public class CreatingRequests
    {

        /// <summary>
        /// Backend for retrieving from webpages and exporting to epubs.
        /// </summary>
        private RetrievingRequests retrieveAndExport;

        /// <summary>
        /// Name of request file.
        /// </summary>
        private string requestFilePath;

        /// <summary>
        /// Construct class with retrieveAndExport backend and name for request.
        /// </summary>
        public CreatingRequests(string requestFileName)
        {
            string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string wetDirectory = Path.Combine(homeDirectory, ".wet");
            string requestFilePath = Path.Combine(wetDirectory, requestFileName);

            if (!Directory.Exists(wetDirectory))
            {
                Directory.CreateDirectory(wetDirectory);
            }
            File.Create(requestFilePath).Dispose();

            retrieveAndExport = new();
            this.requestFilePath = requestFilePath;
        }


        /// <summary>
        /// Send request to retrieve links from webpage according to regex pattern.
        /// </summary>
        public void RequestLinks(string link, Regex regex)
        {
            retrieveAndExport.RetrieveLinks(requestFilePath, link, regex);
        }


        /// <summary>
        /// Displays list of hyperlinks in requestFile.
        /// </summary>
        public void Print()
        {
            IEnumerable<string> lines = File.ReadLines(requestFilePath);
            int i = 0;
            foreach (string line in lines)
            {
                Console.WriteLine($"{i}: {line}");
                i++;
            }

        }

        /// <summary>
        /// Display list of page hyperlinks from start to end.
        /// </summary>
        public void Print(int start, int end)
        {
            IEnumerable<string> lines = File.ReadLines(requestFilePath);
            if (end > lines.Count() || start < 0 || start > end)
            {
                return;
            }

            int i = 0;
            foreach (string line in lines)
            {
                if (i < start)
                {
                    i++;
                    continue;
                }

                if (i > end)
                {
                    break;
                }

                Console.WriteLine($"{i}: {line}");
                i++;
            }
        }

        /// <summary>
        /// Remove page at index.
        /// </summary>
        public void Remove(int index)
        {
            string[] lines = File.ReadAllLines(requestFilePath);
            lines[index].Remove(0);

        }

        /// <summary>
        /// Add page at index.
        /// </summary>
        public void Add(int index, string link)
        {
            string[] lines = File.ReadAllLines(requestFilePath);
            lines[index].Insert(0, link);
        }

        /// <summary>
        /// Reverse list of pages so that index 0 becomes index n and index n becomes index 0.
        /// </summary>
        public void Reverse()
        {
            string[] lines = File.ReadAllLines(requestFilePath);
            lines.Reverse();
        }

        /// <summary>
        /// Assemble epub from each Page.HTML in List<Page> to exportToLocation.
        /// </summary>
        public void ExportToEpub(string exportToLocation)
        {
            retrieveAndExport.ExportToEpub(requestFilePath, exportToLocation);
        }

    }
}
