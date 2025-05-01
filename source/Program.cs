using System.Text.RegularExpressions;
using source.Retrieve_Requests;
using source.Create_Requests;

RetrievingLinks retrievedLinks = new();
CreatingRequests creatingRequests = new(retrievedLinks);

string link = "https://novelbin.com/b/dorothys-forbidden-grimoire#tab-chapters-title";
Regex regex = new("chapter");
