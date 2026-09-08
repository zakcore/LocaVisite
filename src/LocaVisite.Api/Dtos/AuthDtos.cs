using System.ComponentModel.DataAnnotations;

namespace LocaVisite.Api.Dtos;

/// <summary>Identifiants soumis au moment de la connexion.</summary>
public class ConnexionDto
{
    [Required(ErrorMessage = "Le courriel est obligatoire.")]
    [EmailAddress(ErrorMessage = "Le format du courriel n'est pas valide.")]
    public string Courriel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    public string MotDePasse { get; set; } = string.Empty;
}

/// <summary>Réponse retournée après une connexion réussie.</summary>
public class ReponseConnexionDto
{
    public string Jeton { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
