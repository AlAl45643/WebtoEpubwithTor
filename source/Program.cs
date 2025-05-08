using System.Text.RegularExpressions;
using source.Retrieve_Requests;
using source.Create_Requests;

RetrievingRequests retrievedLinks = new();
CreatingRequests creatingRequests = new(retrievedLinks, retrievedLinks);

string link = "https://nhvnovels.com/novels/i-became-the-daughter-of-the-western-general-remake-version/";
Regex regex = new("chapter");

creatingRequests.RequestLinks(link, regex);
creatingRequests.RequestWebpages();
