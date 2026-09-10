using HtmlAgilityPack;

namespace Teleprompter.Services.Scraping;

public interface ISongScraper
{
    Task<string> ScrapeAsync(string url);
}

public class CifraClubScraper : ISongScraper
{
    public async Task<string> ScrapeAsync(string url)
    {
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(url);
        
        // A cifra principal fica tipicamente dentro de uma tag <pre>
        var preNode = doc.DocumentNode.SelectSingleNode("//pre");
        
        if (preNode == null)
            throw new Exception("Não foi possível localizar o bloco de cifra na página.");

        return preNode.InnerText;
    }
}
