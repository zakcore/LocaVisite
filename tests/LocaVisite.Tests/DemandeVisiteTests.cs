using LocaVisite.Api.Controllers;
using LocaVisite.Api.Data;
using LocaVisite.Api.Dtos;
using LocaVisite.Api.Models;
using LocaVisite.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocaVisite.Tests;

/// <summary>
/// Validation de la plage proposée par le prospect, à la réception de la
/// demande.
///
/// Le cas qui a motivé ces tests : une plage de deux minutes était acceptée.
/// La visite était créée, puis restait impossible à assigner — la recherche
/// d'agents la rejetait à chaque fois, sans que personne comprenne pourquoi
/// une demande valide en apparence ne trouvait jamais d'agent.
/// </summary>
public class DemandeVisiteTests
{
    [Fact]
    public async Task PlageDeDeuxMinutes_EstRefusee()
    {
        var controleur = CreerControleur();

        var reponse = await controleur.Demander(Demande("14:00", "14:02"));

        var refus = Assert.IsType<BadRequestObjectResult>(reponse.Result);
        Assert.Contains("2 minutes", Message(refus));
        Assert.Contains("30 minutes", Message(refus));
    }

    [Fact]
    public async Task PlageDeVingtNeufMinutes_EstRefusee()
    {
        var controleur = CreerControleur();

        var reponse = await controleur.Demander(Demande("14:00", "14:29"));

        Assert.IsType<BadRequestObjectResult>(reponse.Result);
    }

    [Fact]
    public async Task PlageDeTrenteMinutesExactement_EstAcceptee()
    {
        var (controleur, contexte) = CreerControleurEtContexte();

        // La borne est inclusive : trente minutes suffisent tout juste à
        // loger une visite de trente minutes.
        var reponse = await controleur.Demander(Demande("14:00", "14:30"));

        var succes = Assert.IsType<OkObjectResult>(reponse.Result);
        var donnees = Assert.IsType<ReponseDemandeVisiteDto>(succes.Value);

        Assert.Equal("DEMANDEE", donnees.Statut);
        Assert.Equal(Visite.DureeParDefautMinutes, contexte.Visites.Single().DureePrevue);
    }

    [Fact]
    public async Task PlageLarge_EstAcceptee()
    {
        var controleur = CreerControleur();

        var reponse = await controleur.Demander(Demande("14:00", "16:00"));

        Assert.IsType<OkObjectResult>(reponse.Result);
    }

    [Fact]
    public async Task PlageInversee_EstRefuseeAvantLeControleDeDuree()
    {
        var controleur = CreerControleur();

        // Une plage à l'envers doit garder son message propre plutôt que de
        // se faire rejeter comme « trop courte » avec une durée négative.
        var reponse = await controleur.Demander(Demande("16:00", "14:00"));

        var refus = Assert.IsType<BadRequestObjectResult>(reponse.Result);
        Assert.Contains("postérieure", Message(refus));
    }

    /// <summary>
    /// Une plage refusée ne doit rien laisser derrière elle : ni visite
    /// orpheline, ni prospect créé pour rien.
    /// </summary>
    [Fact]
    public async Task PlageTropCourte_NeCreeNiVisiteNiProspect()
    {
        var (controleur, contexte) = CreerControleurEtContexte();

        await controleur.Demander(Demande("14:00", "14:02"));

        Assert.Empty(contexte.Visites);
        Assert.Empty(contexte.Prospects);
    }

    // --- Utilitaires ---------------------------------------------------------

    private static string Message(ObjectResult resultat)
    {
        var propriete = resultat.Value!.GetType().GetProperty("message");
        return propriete!.GetValue(resultat.Value)!.ToString()!;
    }

    private static DemandeVisiteDto Demande(string debut, string fin) => new()
    {
        IdLogement = 1,
        Nom = "Bouchard",
        Prenom = "Julie",
        Courriel = "julie.bouchard@example.ca",
        Telephone = "450-555-0142",
        PossedeMobile = true,
        DateSouhaitee = DateOnly.FromDateTime(DateTime.Today).AddDays(14),
        HeureSouhaiteeDebut = TimeOnly.Parse(debut),
        HeureSouhaiteeFin = TimeOnly.Parse(fin),
    };

    private static VisitesController CreerControleur() => CreerControleurEtContexte().Controleur;

    private static (VisitesController Controleur, LocaVisiteContext Contexte) CreerControleurEtContexte()
    {
        var options = new DbContextOptionsBuilder<LocaVisiteContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var contexte = new LocaVisiteContext(options);

        contexte.Logements.Add(new Logement
        {
            IdLogement = 1,
            Adresse = "125 rue Richelieu",
            Ville = "Saint-Jean-sur-Richelieu",
            CodePostal = "J3B 6X3",
            Type = "Appartement",
            NbPieces = 4,
            LoyerMensuel = 950m,
            Statut = StatutLogement.DISPONIBLE,
        });

        contexte.SaveChanges();

        return (new VisitesController(contexte, new ServiceRechercheAgents(contexte)), contexte);
    }
}
