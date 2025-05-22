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
        public CreatingRequests(string requestFileName, bool trace)
        {
            string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string wetDirectory = Path.Combine(homeDirectory, ".wet");
            string requestFilePath = Path.Combine(wetDirectory, requestFileName);

            if (!Directory.Exists(wetDirectory))
            {
                _ = Directory.CreateDirectory(wetDirectory);
            }
            if (!File.Exists(requestFilePath))
            {
                File.Create(requestFilePath).Dispose();
            }

            retrieveAndExport = new(trace);
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
            string[] lines = File.ReadAllLines(requestFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                Console.WriteLine($"{i}: {lines[i]}");
            }

        }

        /// <summary>
        /// Display list of page hyperlinks from <paramref name="start"/> to <paramref name="end"/>.
        /// </summary>
        public void Print(int start, int end)
        {
            string[] lines = File.ReadAllLines(requestFilePath);
            if (end > lines.Length || start < 0 || start > end)
            {
                return;
            }
            for (int i = start; i < end; i++)
            {
                Console.WriteLine($"{i}: {lines[i]}");
            }
        }

        /// <summary>
        /// Remove link at <paramref name="index"/>.
        /// </summary>
        public void RemoveAt(int index)
        {
            List<string> lines = [.. File.ReadAllLines(requestFilePath)];
            lines.RemoveAt(index);
            File.WriteAllLines(requestFilePath, lines);
        }

        /// <summary>
        /// Add link at <paramref name="index"/>.
        /// </summary>
        public void Add(int index, string link)
        {
            List<string> lines = [.. File.ReadAllLines(requestFilePath)];
            lines.Insert(index, link);
            File.WriteAllLines(requestFilePath, lines);
        }

        /// <summary>
        /// Reverse request file so that index 0 becomes index n and index n becomes index 0.
        /// </summary>
        public void Reverse()
        {
            List<string> lines = [.. File.ReadAllLines(requestFilePath)];
            lines.Reverse();
            File.WriteAllLines(requestFilePath, lines);
        }

        /// <summary>
        /// Remove links in list from index <paramref name="begin"/> to index <paramref name="end"/>.
        /// </summary>
        public void RemoveFrom(int begin, int end)
        {
            List<string> lines = [.. File.ReadAllLines(requestFilePath)];
            int count = end - begin;
            lines.RemoveRange(begin, count);
            File.WriteAllLines(requestFilePath, lines);
        }

        /// <summary>
        /// Clears all links in the request.
        /// </summary>
        public void Clear()
        {
            File.WriteAllText(requestFilePath, string.Empty);
        }

        /// <summary>
        /// Lists all requests created.
        /// </summary>
        public static void ListRequests()
        {
            string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string wetDirectory = Path.Combine(homeDirectory, ".wet");
            IEnumerable<string> requests = Directory.EnumerateFiles(wetDirectory);
            foreach (string request in requests)
            {
                Console.WriteLine(Path.GetFileName(request));
            }
        }

        /// <summary>
        /// Assemble epub from each link in request to <paramref name="exportToLocation"/>.
        /// </summary>
        public void ExportRequest(string exportToLocation)
        {
            retrieveAndExport.ExportToEpub(requestFilePath, exportToLocation);
        }

    }
}
