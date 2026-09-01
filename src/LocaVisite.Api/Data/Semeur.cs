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
    }
}
