using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LocaVisite.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace LocaVisite.Api.Services;

/// <summary>Génère les jetons JWT remis aux utilisateurs authentifiés.</summary>
public class ServiceJeton
{
    private readonly IConfiguration _configuration;

    public ServiceJeton(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>Produit un jeton porteur contenant l'identifiant, le courriel et le rôle.</summary>
    public string GenererJeton(Utilisateur utilisateur)
    {
        var section = _configuration.GetSection("Jwt");
        var cle = section["Cle"];

        if (string.IsNullOrWhiteSpace(cle))
        {
            throw new InvalidOperationException(
                "La clé JWT est absente. Exécutez : dotnet user-secrets set \"Jwt:Cle\" \"<votre clé>\".");
        }

        var dureeMinutes = int.TryParse(section["DureeMinutes"], out var duree) ? duree : 480;

        var revendications = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, utilisateur.IdUtilisateur.ToString()),
            new(ClaimTypes.NameIdentifier, utilisateur.IdUtilisateur.ToString()),
            new(ClaimTypes.Email, utilisateur.Courriel),
            new(ClaimTypes.Role, utilisateur.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var cleSignature = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cle));
        var identifiants = new SigningCredentials(cleSignature, SecurityAlgorithms.HmacSha256);

        var jeton = new JwtSecurityToken(
            issuer: section["Emetteur"],
            audience: section["Audience"],
            claims: revendications,
            expires: DateTime.UtcNow.AddMinutes(dureeMinutes),
            signingCredentials: identifiants);

        return new JwtSecurityTokenHandler().WriteToken(jeton);
    }
}
