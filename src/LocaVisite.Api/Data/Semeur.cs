using LocaVisite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LocaVisite.Api.Data;

/// <summary>Insère les données de départ au démarrage, sans créer de doublon.</summary>
public static class Semeur
{
    public static void Semer(LocaVisiteContext contexte)
    {
        if (!contexte.Utilisateurs.Any(u => u.Courriel == "prepose@locavisite.ca"))
        {
            contexte.Utilisateurs.Add(new Utilisateur
            {
                Nom = "Tremblay",
                Prenom = "Marie",
                Courriel = "prepose@locavisite.ca",
                MotDePasse = BCrypt.Net.BCrypt.HashPassword("Test1234!"),
                Telephone = "450-555-0101",
                Role = Role.PREPOSE,
                FournisseurAuth = FournisseurAuth.LOCAL
            });
        }

        if (!contexte.Utilisateurs.Any(u => u.Courriel == "agent@locavisite.ca"))
        {
            contexte.Utilisateurs.Add(new Utilisateur
            {
                Nom = "Gagnon",
                Prenom = "Luc",
                Courriel = "agent@locavisite.ca",
                MotDePasse = BCrypt.Net.BCrypt.HashPassword("Test1234!"),
                Telephone = "450-555-0102",
                Role = Role.AGENT,
                FournisseurAuth = FournisseurAuth.LOCAL
            });
        }

        if (!contexte.Logements.Any())
        {
            contexte.Logements.AddRange(
                new Logement
                {
                    Adresse = "125 rue Richelieu",
                    Ville = "Saint-Jean-sur-Richelieu",
                    CodePostal = "J3B 6X3",
                    Type = "Appartement",
                    NbPieces = 4,
                    LoyerMensuel = 950m,
                    Description = "Quatre et demi rénové, proche du centre-ville et du Richelieu.",
                    Statut = StatutLogement.DISPONIBLE,
                    Latitude = 45.3073,
                    Longitude = -73.2620
                },
                new Logement
                {
                    Adresse = "48 boulevard du Séminaire Nord",
                    Ville = "Saint-Jean-sur-Richelieu",
                    CodePostal = "J3B 5J4",
                    Type = "Condo",
                    NbPieces = 5,
                    LoyerMensuel = 1250m,
                    Description = "Condo de cinq et demi avec stationnement inclus.",
                    Statut = StatutLogement.DISPONIBLE,
                    Latitude = 45.3181,
                    Longitude = -73.2637
                });
        }

        contexte.SaveChanges();

        SemerDisponibilites(contexte);
        SemerVisiteDeReference(contexte);
    }

    /// <summary>
    /// Sème la visite qui occupe l'agent dans le cas de référence de SPECIFICATIONS.md :
    /// mardi 15 septembre 2026, de 15:00 à 15:30.
    ///
    /// Sans elle l'agent serait libre toute la plage, et la recherche
    /// retournerait tous les créneaux au lieu du seul 14:00 attendu.
    /// </summary>
    private static void SemerVisiteDeReference(LocaVisiteContext contexte)
    {
        var dateReference = new DateOnly(2026, 9, 15);
        var heureReference = new TimeOnly(15, 0);

        var agent = contexte.Utilisateurs
            .FirstOrDefault(u => u.Courriel == "agent@locavisite.ca");

        var logement = contexte.Logements
            .FirstOrDefault(l => l.Statut == StatutLogement.DISPONIBLE);

        if (agent is null || logement is null)
        {
            return;
        }

        var dejaSemee = contexte.Visites.Any(v =>
            v.IdAgent == agent.IdUtilisateur
            && v.DatePrevue == dateReference
            && v.HeurePrevue == heureReference);

        if (dejaSemee)
        {
            return;
        }

        // Un prospect distinct de ceux des demandes, pour que le cas de
        // référence ne dépende pas des essais faits depuis Swagger.
        var prospect = contexte.Prospects
            .FirstOrDefault(p => p.Courriel == "occupant@locavisite.ca");

        if (prospect is null)
        {
            prospect = new Prospect
            {
                Nom = "Lavoie",
                Prenom = "Simon",
                Courriel = "occupant@locavisite.ca",
                Telephone = "450-555-0188",
                PossedeMobile = true,
                DateCreation = DateTime.Now
            };

            contexte.Prospects.Add(prospect);
        }

        contexte.Visites.Add(new Visite
        {
            IdLogement = logement.IdLogement,
            Prospect = prospect,
            IdAgent = agent.IdUtilisateur,
            DateSouhaitee = dateReference,
            HeureSouhaiteeDebut = heureReference,
            HeureSouhaiteeFin = new TimeOnly(16, 0),
            DatePrevue = dateReference,
            HeurePrevue = heureReference,
            DureePrevue = 30,
            Statut = StatutVisite.ASSIGNEE
        });

        contexte.SaveChanges();
    }

    /// <summary>
    /// Donne à l'agent semé un horaire du lundi au vendredi, de 9:00 à 17:00,
    /// pour que la recherche d'agents disponibles ait de quoi travailler.
    /// </summary>
    private static void SemerDisponibilites(LocaVisiteContext contexte)
    {
        var agent = contexte.Utilisateurs
            .FirstOrDefault(u => u.Courriel == "agent@locavisite.ca");

        if (agent is null)
        {
            return;
        }

        var joursDejaSemes = contexte.Disponibilites
            .Where(d => d.IdUtilisateur == agent.IdUtilisateur)
            .Select(d => d.JourSemaine)
            .ToList();

        // 1 = lundi ... 5 = vendredi, selon la convention de DayOfWeek.
        for (var jour = 1; jour <= 5; jour++)
        {
            if (joursDejaSemes.Contains(jour))
            {
                continue;
            }

            contexte.Disponibilites.Add(new Disponibilite
            {
                IdUtilisateur = agent.IdUtilisateur,
                JourSemaine = jour,
                HeureDebut = new TimeOnly(9, 0),
                HeureFin = new TimeOnly(17, 0)
            });
        }

        contexte.SaveChanges();
    }
}
