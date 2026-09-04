using LocaVisite.Api.Data;
using LocaVisite.Api.Models;
using LocaVisite.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace LocaVisite.Tests;

/// <summary>
/// Les huit cas de test de SPECIFICATIONS.md, plus le cas de référence.
///
/// Le 15 septembre 2026 est un mardi : c'est la date utilisée partout ici,
/// pour que les disponibilités du mardi (jour 2) s'appliquent.
/// </summary>
public class ServiceRechercheAgentsTests
{
    private static readonly DateOnly Mardi = new(2026, 9, 15);
    private const int MardiJourSemaine = 2;

    // --- Cas 1 ---------------------------------------------------------------

    [Fact]
    public async Task Cas1_AgentSansDisponibiliteCeJourLa_NApparaitPas()
    {
        using var contexte = CreerBase();
        var agent = AjouterAgent(contexte, 10, "Luc", "Gagnon");

        // L'agent ne travaille que le lundi, la visite est demandée un mardi.
        AjouterDisponibilite(contexte, agent, jourSemaine: 1, "09:00", "17:00");

        var visite = AjouterVisiteDemandee(contexte, "14:00", "16:00", duree: 30);
        var resultat = await Chercher(contexte, visite);

        Assert.Empty(resultat.Creneaux);
        Assert.Equal("Aucun agent n'a déclaré de disponibilité ce jour-là.", resultat.Raison);
    }

    // --- Cas 2 ---------------------------------------------------------------

    [Fact]
    public async Task Cas2_PlageEntierementHorsDesHeuresDeLAgent_NApparaitPas()
    {
        using var contexte = CreerBase();
        var agent = AjouterAgent(contexte, 10, "Luc", "Gagnon");

        // L'agent finit à 12:00, la plage demandée commence à 14:00.
        AjouterDisponibilite(contexte, agent, MardiJourSemaine, "09:00", "12:00");

        var visite = AjouterVisiteDemandee(contexte, "14:00", "16:00", duree: 30);
        var resultat = await Chercher(contexte, visite);

        Assert.Empty(resultat.Creneaux);
        Assert.Equal("Aucun agent n'est libre pendant la plage demandée.", resultat.Raison);
    }

    // --- Cas 3 ---------------------------------------------------------------

    [Fact]
    public async Task Cas3_PlagePlusCourteQueLaDuree_ListeVideAvecRaison()
    {
        using var contexte = CreerBase();
        var agent = AjouterAgent(contexte, 10, "Luc", "Gagnon");
        AjouterDisponibilite(contexte, agent, MardiJourSemaine, "09:00", "17:00");

        // 20 minutes de plage pour une visite qui en demande 30.
        var visite = AjouterVisiteDemandee(contexte, "14:00", "14:20", duree: 30);
        var resultat = await Chercher(contexte, visite);

        Assert.Empty(resultat.Creneaux);
        Assert.Equal("La plage demandée dure 20 minutes, la visite en requiert 30.", resultat.Raison);
    }

    // --- Cas 4 ---------------------------------------------------------------

    [Fact]
    public async Task Cas4_AgentLibreTouteLaPlage_CreneauxTousLes15Minutes()
    {
        using var contexte = CreerBase();
        var agent = AjouterAgent(contexte, 10, "Luc", "Gagnon");
        AjouterDisponibilite(contexte, agent, MardiJourSemaine, "09:00", "17:00");

        var visite = AjouterVisiteDemandee(contexte, "14:00", "16:00", duree: 30);
        var resultat = await Chercher(contexte, visite);

        // Le dernier créneau est 15:30 : à 15:45 la visite dépasserait 16:00.
        Assert.Equal(
            new[] { "14:00", "14:15", "14:30", "14:45", "15:00", "15:15", "15:30" },
            resultat.Creneaux.Select(c => c.HeureDebut));
        Assert.Null(resultat.Raison);
    }

    // --- Cas 5 ---------------------------------------------------------------

    [Fact]
    public async Task Cas5_VisiteExistanteAuMilieu_CreneauxAvantEtApresAvecTampon()
    {
        using var contexte = CreerBase();
        var agent = AjouterAgent(contexte, 10, "Luc", "Gagnon");
        AjouterDisponibilite(contexte, agent, MardiJourSemaine, "09:00", "17:00");
        AjouterVisiteOccupante(contexte, agent, "12:00", duree: 30);

        var visite = AjouterVisiteDemandee(contexte, "11:00", "14:00", duree: 30);
        var resultat = await Chercher(contexte, visite);

        // Avant : 11:00 finit à 11:30, soit 30 minutes pile avant 12:00.
        //         11:15 finirait à 11:45, il ne resterait que 15 minutes.
        // Après : 12:30 démarrerait pile à la fin de la visite, écart nul ;
        //         12:45 ne laisserait que 15 minutes ; 13:00 en laisse 30.
        Assert.Equal(
            new[] { "11:00", "13:00", "13:15", "13:30" },
            resultat.Creneaux.Select(c => c.HeureDebut));
    }

    // --- Cas 6 ---------------------------------------------------------------

    [Fact]
    public async Task Cas6_VisiteAnnulee_NOccupePasLAgent()
    {
        using var contexte = CreerBase();
        var agent = AjouterAgent(contexte, 10, "Luc", "Gagnon");
        AjouterDisponibilite(contexte, agent, MardiJourSemaine, "09:00", "17:00");

        // Même visite que le cas de référence, mais annulée : elle ne bloque rien.
        AjouterVisiteOccupante(contexte, agent, "15:00", duree: 30, statut: StatutVisite.ANNULEE);

        var visite = AjouterVisiteDemandee(contexte, "14:00", "16:00", duree: 30);
        var resultat = await Chercher(contexte, visite);

        Assert.Equal(
            new[] { "14:00", "14:15", "14:30", "14:45", "15:00", "15:15", "15:30" },
            resultat.Creneaux.Select(c => c.HeureDebut));
    }

    // --- Cas 7 ---------------------------------------------------------------

    [Fact]
    public async Task Cas7_DeuxAgentsLibres_LeMoinsChargeEnPremier()
    {
        using var contexte = CreerBase();
        var charge = AjouterAgent(contexte, 10, "Luc", "Gagnon");
        var libre = AjouterAgent(contexte, 11, "Sophie", "Roy");

        AjouterDisponibilite(contexte, charge, MardiJourSemaine, "09:00", "17:00");
        AjouterDisponibilite(contexte, libre, MardiJourSemaine, "09:00", "17:00");

        // Une visite le matin : elle ne bloque pas l'après-midi, mais elle
        // rend cet agent plus chargé que l'autre.
        AjouterVisiteOccupante(contexte, charge, "09:00", duree: 30);

        var visite = AjouterVisiteDemandee(contexte, "14:00", "15:00", duree: 30);
        var resultat = await Chercher(contexte, visite);

        // Trois heures de début tiennent dans la plage (14:00, 14:15 et 14:30,
        // la visite devant finir avant 15:00), pour chacun des deux agents.
        Assert.Equal(6, resultat.Creneaux.Count);
        Assert.Equal(
            new[] { "14:00", "14:00", "14:15", "14:15", "14:30", "14:30" },
            resultat.Creneaux.Select(c => c.HeureDebut));

        // À heure égale, l'agent le moins chargé passe devant.
        var a14h = resultat.Creneaux.Where(c => c.HeureDebut == "14:00").ToList();
        Assert.Equal(libre.IdUtilisateur, a14h[0].IdAgent);
        Assert.Equal(charge.IdUtilisateur, a14h[1].IdAgent);
    }

    // --- Cas 8 ---------------------------------------------------------------

    [Fact]
    public async Task Cas8_JourneeEntierementReservee_ListeVideAvecRaison()
    {
        using var contexte = CreerBase();
        var agent = AjouterAgent(contexte, 10, "Luc", "Gagnon");
        AjouterDisponibilite(contexte, agent, MardiJourSemaine, "09:00", "17:00");

        // Une visite de 13:00 à 17:00 recouvre toute la plage demandée.
        AjouterVisiteOccupante(contexte, agent, "13:00", duree: 240);

        var visite = AjouterVisiteDemandee(contexte, "14:00", "16:00", duree: 30);
        var resultat = await Chercher(contexte, visite);

        Assert.Empty(resultat.Creneaux);
        Assert.Equal("Aucun agent n'est libre pendant la plage demandée.", resultat.Raison);
    }

    // --- Cas de référence ----------------------------------------------------

    /// <summary>
    /// Le test le plus important du projet : l'exemple de référence de SPECIFICATIONS.md.
    ///
    /// Mardi 15 septembre 2026, agent disponible 9:00–17:00, visite existante
    /// 15:00–15:30, plage souhaitée 14:00–16:00, durée requise 30 minutes.
    /// Un seul créneau valide : 14:00.
    /// </summary>
    [Fact]
    public async Task CasDeReference_UnSeulCreneauA14h()
    {
        using var contexte = CreerBase();
        var agent = AjouterAgent(contexte, 10, "Luc", "Gagnon");
        AjouterDisponibilite(contexte, agent, MardiJourSemaine, "09:00", "17:00");
        AjouterVisiteOccupante(contexte, agent, "15:00", duree: 30);

        var visite = AjouterVisiteDemandee(contexte, "14:00", "16:00", duree: 30);
        var resultat = await Chercher(contexte, visite);

        var creneau = Assert.Single(resultat.Creneaux);
        Assert.Equal("14:00", creneau.HeureDebut);
        Assert.Equal("14:30", creneau.HeureFin);
        Assert.Equal(agent.IdUtilisateur, creneau.IdAgent);
        Assert.Equal("Luc Gagnon", creneau.NomAgent);
        Assert.Null(resultat.Raison);

        // Détail attendu, créneau par créneau :
        var heures = resultat.Creneaux.Select(c => c.HeureDebut).ToList();

        // 14:00 retenu — finit à 14:30, écart de 30 minutes pile avant 15:00.
        Assert.Contains("14:00", heures);

        // 14:15 rejeté — finirait à 14:45, il ne resterait que 15 minutes.
        Assert.DoesNotContain("14:15", heures);

        // 14:30 rejeté — finirait pile à 15:00, écart nul.
        Assert.DoesNotContain("14:30", heures);

        // 14:45, 15:00 et 15:15 rejetés — chevauchent la visite de 15:00 à 15:30.
        Assert.DoesNotContain("14:45", heures);
        Assert.DoesNotContain("15:00", heures);
        Assert.DoesNotContain("15:15", heures);

        // 15:30 rejeté — démarrerait à l'instant où la précédente finit, écart nul.
        Assert.DoesNotContain("15:30", heures);
    }

    // --- Utilitaires ---------------------------------------------------------

    private static LocaVisiteContext CreerBase()
    {
        var options = new DbContextOptionsBuilder<LocaVisiteContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new LocaVisiteContext(options);
    }

    private static async Task<Api.Dtos.AgentsDisponiblesDto> Chercher(
        LocaVisiteContext contexte,
        Visite visite)
    {
        contexte.SaveChanges();
        return await new ServiceRechercheAgents(contexte).ChercherAsync(visite);
    }

    private static Utilisateur AjouterAgent(LocaVisiteContext contexte, int id, string prenom, string nom)
    {
        var agent = new Utilisateur
        {
            IdUtilisateur = id,
            Nom = nom,
            Prenom = prenom,
            Courriel = $"agent{id}@locavisite.ca",
            MotDePasse = "peu importe",
            Role = Role.AGENT
        };

        contexte.Utilisateurs.Add(agent);
        return agent;
    }

    private static void AjouterDisponibilite(
        LocaVisiteContext contexte,
        Utilisateur agent,
        int jourSemaine,
        string debut,
        string fin)
    {
        contexte.Disponibilites.Add(new Disponibilite
        {
            IdUtilisateur = agent.IdUtilisateur,
            JourSemaine = jourSemaine,
            HeureDebut = TimeOnly.Parse(debut),
            HeureFin = TimeOnly.Parse(fin)
        });
    }

    /// <summary>Visite déjà prévue à l'horaire d'un agent, le mardi de référence.</summary>
    private static void AjouterVisiteOccupante(
        LocaVisiteContext contexte,
        Utilisateur agent,
        string heurePrevue,
        int duree,
        StatutVisite statut = StatutVisite.ASSIGNEE)
    {
        contexte.Visites.Add(new Visite
        {
            IdLogement = 1,
            IdProspect = 1,
            IdAgent = agent.IdUtilisateur,
            DateSouhaitee = Mardi,
            HeureSouhaiteeDebut = TimeOnly.Parse(heurePrevue),
            HeureSouhaiteeFin = TimeOnly.Parse(heurePrevue).AddMinutes(duree),
            DatePrevue = Mardi,
            HeurePrevue = TimeOnly.Parse(heurePrevue),
            DureePrevue = duree,
            Statut = statut
        });
    }

    /// <summary>La visite pour laquelle on cherche un agent : DEMANDEE, sans agent.</summary>
    private static Visite AjouterVisiteDemandee(
        LocaVisiteContext contexte,
        string debut,
        string fin,
        int duree)
    {
        var visite = new Visite
        {
            IdLogement = 1,
            IdProspect = 1,
            IdAgent = null,
            DateSouhaitee = Mardi,
            HeureSouhaiteeDebut = TimeOnly.Parse(debut),
            HeureSouhaiteeFin = TimeOnly.Parse(fin),
            DureePrevue = duree,
            Statut = StatutVisite.DEMANDEE
        };

        contexte.Visites.Add(visite);
        return visite;
    }
}
