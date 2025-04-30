using System.Text.RegularExpressions;
using source.Retrieve_Requests;

RetrievedLinks retrievedLinks = new RetrievedLinks();
Regex regex = new Regex("chapter");

List<source.Page> listOfPages = retrievedLinks.RetrieveLinks("https://fenrirealm.com/series/dorothys-forbidden-grimoire", regex);

foreach (source.Page page in listOfPages)
{
    Console.WriteLine(page.Hyperlink);
    Console.WriteLine(page.PageContent);
}
