using Microsoft.EntityFrameworkCore;
using Teleprompter.Core.Models;
using Teleprompter.Data.Context;
using Teleprompter.Services.Scraping;

namespace Teleprompter.Wpf;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, ISongScraper scraper)
    {
        // Verifica se o banco atual é compatível com o modelo (ex: falta da coluna Album)
        bool requiresRecreate = false;
        try
        {
            db.Database.EnsureCreated();
            _ = db.Songs.FirstOrDefault(); // Testa se a tabela possui todas as colunas
        }
        catch (Exception)
        {
            requiresRecreate = true;
        }

        if (requiresRecreate)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }

        // Seed inicial com links reais do CifraClub
        if (!db.Songs.Any())
        {
            try
            {
                string rawEvidencias = await scraper.ScrapeAsync("https://www.cifraclub.com.br/chitaozinho-e-xororo/evidencias/");
                string rawSina = await scraper.ScrapeAsync("https://www.cifraclub.com.br/djavan/sina/");
                string rawMare = await scraper.ScrapeAsync("https://www.cifraclub.com.br/jorge-vercillo/que-nem-mare/");

                db.Songs.Add(new Song { Title = "Evidências", Artist = "Chitãozinho & Xororó", Album = "Cowboy do Asfalto", RawContent = rawEvidencias });
                db.Songs.Add(new Song { Title = "Sina", Artist = "Djavan", Album = "Luz", RawContent = rawSina });
                db.Songs.Add(new Song { Title = "Que Nem Maré", Artist = "Jorge Vercillo", Album = "Perfil", RawContent = rawMare });
                
                await db.SaveChangesAsync();
            }
            catch 
            { 
                /* fallback silencioso caso falhe a internet */ 
            }
        }
    }
}
