using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocaVisite.Api.Models;

/// <summary>
/// Visite d'un logement par un prospect. Elle existe dès la demande,
/// avant qu'un agent lui soit assigné.
/// </summary>
[Table("VISITE")]
public class Visite
{
    /// <summary>
    /// Durée par défaut d'une visite, en minutes. C'est aussi la longueur
    /// minimale qu'une plage proposée doit avoir : une plage plus courte ne
    /// pourrait jamais accueillir de visite.
    /// </summary>
    public const int DureeParDefautMinutes = 30;

    [Key]
    [Column("id_visite")]
    public int IdVisite { get; set; }

    [Column("id_logement")]
    public int IdLogement { get; set; }

    [Column("id_prospect")]
    public int IdProspect { get; set; }

    /// <summary>Nul tant que la visite n'est pas assignée à un agent.</summary>
    [Column("id_agent")]
    public int? IdAgent { get; set; }

    // --- Plage proposée par le prospect ---

    [Column("date_souhaitee")]
    public DateOnly DateSouhaitee { get; set; }

    [Column("heure_souhaitee_debut")]
    public TimeOnly HeureSouhaiteeDebut { get; set; }

    [Column("heure_souhaitee_fin")]
    public TimeOnly HeureSouhaiteeFin { get; set; }

    // --- Horaire retenu par le préposé, nul avant l'assignation ---

    [Column("date_prevue")]
    public DateOnly? DatePrevue { get; set; }

    [Column("heure_prevue")]
    public TimeOnly? HeurePrevue { get; set; }

    /// <summary>Durée prévue de la visite, en minutes.</summary>
    [Column("duree_prevue")]
    public int DureePrevue { get; set; } = DureeParDefautMinutes;

    [Column("statut")]
    public StatutVisite Statut { get; set; } = StatutVisite.DEMANDEE;

    // --- Déplacement de l'agent ---

    [Column("lieu_origine")]
    [MaxLength(200)]
    public string? LieuOrigine { get; set; }

    [Column("distance_km")]
    public decimal? DistanceKm { get; set; }

    [Column("heure_arrivee_prevue")]
    public TimeOnly? HeureArriveePrevue { get; set; }

    // --- Temps réellement passé sur place ---

    [Column("heure_punch_in")]
    public DateTime? HeurePunchIn { get; set; }

    [Column("heure_punch_out")]
    public DateTime? HeurePunchOut { get; set; }

    /// <summary>Durée réelle de la visite, en minutes.</summary>
    [Column("duree_reelle")]
    public int? DureeReelle { get; set; }

    public Logement? Logement { get; set; }

    public Prospect? Prospect { get; set; }

    public Utilisateur? Agent { get; set; }

    /// <summary>Rapport rédigé après la visite. Relation 1—0..1.</summary>
    public RapportVisite? RapportVisite { get; set; }

    public ICollection<Communication> Communications { get; set; } = new List<Communication>();
}
