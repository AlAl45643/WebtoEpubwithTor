using source.Retrieve_Requests;
using source.Create_Requests;
using System.Text.RegularExpressions;
using source;
RetrievingRequests retrievedLinks = new();
CreatingRequests creatingRequests = new(retrievedLinks);

string link = "https://pienovels.com/novels/i-hate-it-when-oo-intrudes-in-a-yuri-story/";
Regex regex = new("chapter");

creatingRequests.RequestLinks(link, regex);
creatingRequests.Print();
creatingRequests.RequestWebpages();

creatingRequests.ExportToEpub("/home/work/Downloads/epub.epub");


// internal class Program
// {
//     static void Main(string[] args)
//     {
//         if (args.Length == 0)
//         {


//         }
//     }
// }
