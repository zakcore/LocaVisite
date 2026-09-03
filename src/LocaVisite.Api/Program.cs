using System.Text;
using System.Text.Json.Serialization;
using LocaVisite.Api.Data;
using LocaVisite.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Base de données ---
builder.Services.AddDbContext<LocaVisiteContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocaVisite")));

// --- Authentification par jeton porteur ---
var sectionJwt = builder.Configuration.GetSection("Jwt");
var cleJwt = sectionJwt["Cle"];

if (string.IsNullOrWhiteSpace(cleJwt))
{
    throw new InvalidOperationException(
        "La clé JWT est absente. Exécutez : dotnet user-secrets set \"Jwt:Cle\" \"<votre clé d'au moins 32 caractères>\".");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = sectionJwt["Emetteur"],
            ValidAudience = sectionJwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cleJwt))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<ServiceJeton>();
builder.Services.AddScoped<ServiceRechercheAgents>();

// --- CORS : le serveur de développement Vite tourne sur un autre port que
//     l'API, le navigateur refuse donc les appels sans cette autorisation. ---
const string PolitiqueWeb = "ClientsWeb";

builder.Services.AddCors(options =>
{
    options.AddPolicy(PolitiqueWeb, politique =>
    {
        politique
            .WithOrigins(
                builder.Configuration.GetSection("OriginesAutorisees").Get<string[]>()
                ?? ["http://localhost:5173"])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // L'API retourne déjà les énumérations en texte (« DISPONIBLE ») :
        // ce convertisseur les accepte aussi en entrée, sans quoi un aller-retour
        // lecture puis écriture échouerait.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// --- Swagger, avec le bouton « Authorize » pour coller un jeton ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LocaVisite",
        Version = "v1",
        Description = "Services Web de gestion des visites de logements locatifs."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Collez uniquement le jeton retourné par /api/auth/connexion."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// --- Migration et données de départ appliquées au démarrage ---
using (var portee = app.Services.CreateScope())
{
    var contexte = portee.ServiceProvider.GetRequiredService<LocaVisiteContext>();
    contexte.Database.Migrate();
    Semeur.Semer(contexte);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "LocaVisite v1");
        // Swagger s'ouvre directement à la racine : https://localhost:7222/
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// Avant l'authentification : la requête préalable (preflight) du navigateur
// ne porte pas de jeton et doit tout de même recevoir une réponse.
app.UseCors(PolitiqueWeb);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
