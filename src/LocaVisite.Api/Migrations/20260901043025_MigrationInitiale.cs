using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocaVisite.Api.Migrations
{
    /// <inheritdoc />
    public partial class MigrationInitiale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LOGEMENT",
                columns: table => new
                {
                    id_logement = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    adresse = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ville = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    code_postal = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    nb_pieces = table.Column<int>(type: "int", nullable: false),
                    loyer_mensuel = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    statut = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    latitude = table.Column<double>(type: "float", nullable: true),
                    longitude = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOGEMENT", x => x.id_logement);
                });

            migrationBuilder.CreateTable(
                name: "PROSPECT",
                columns: table => new
                {
                    id_prospect = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    prenom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    courriel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    telephone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    possede_mobile = table.Column<bool>(type: "bit", nullable: false),
                    date_creation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROSPECT", x => x.id_prospect);
                });

            migrationBuilder.CreateTable(
                name: "UTILISATEUR",
                columns: table => new
                {
                    id_utilisateur = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    prenom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    courriel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    mot_de_passe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    telephone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fournisseur_auth = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    id_externe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UTILISATEUR", x => x.id_utilisateur);
                });

            migrationBuilder.CreateTable(
                name: "PHOTO",
                columns: table => new
                {
                    id_photo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_logement = table.Column<int>(type: "int", nullable: false),
                    chemin_fichier = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ordre_affichage = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PHOTO", x => x.id_photo);
                    table.ForeignKey(
                        name: "FK_PHOTO_LOGEMENT_id_logement",
                        column: x => x.id_logement,
                        principalTable: "LOGEMENT",
                        principalColumn: "id_logement",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DISPONIBILITE",
                columns: table => new
                {
                    id_disponibilite = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_utilisateur = table.Column<int>(type: "int", nullable: false),
                    jour_semaine = table.Column<int>(type: "int", nullable: false),
                    heure_debut = table.Column<TimeOnly>(type: "time", nullable: false),
                    heure_fin = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DISPONIBILITE", x => x.id_disponibilite);
                    table.ForeignKey(
                        name: "FK_DISPONIBILITE_UTILISATEUR_id_utilisateur",
                        column: x => x.id_utilisateur,
                        principalTable: "UTILISATEUR",
                        principalColumn: "id_utilisateur",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VISITE",
                columns: table => new
                {
                    id_visite = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_logement = table.Column<int>(type: "int", nullable: false),
                    id_prospect = table.Column<int>(type: "int", nullable: false),
                    id_agent = table.Column<int>(type: "int", nullable: true),
                    date_souhaitee = table.Column<DateOnly>(type: "date", nullable: false),
                    heure_souhaitee_debut = table.Column<TimeOnly>(type: "time", nullable: false),
                    heure_souhaitee_fin = table.Column<TimeOnly>(type: "time", nullable: false),
                    date_prevue = table.Column<DateOnly>(type: "date", nullable: true),
                    heure_prevue = table.Column<TimeOnly>(type: "time", nullable: true),
                    duree_prevue = table.Column<int>(type: "int", nullable: false),
                    statut = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    lieu_origine = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    distance_km = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    heure_arrivee_prevue = table.Column<TimeOnly>(type: "time", nullable: true),
                    heure_punch_in = table.Column<DateTime>(type: "datetime2", nullable: true),
                    heure_punch_out = table.Column<DateTime>(type: "datetime2", nullable: true),
                    duree_reelle = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VISITE", x => x.id_visite);
                    table.ForeignKey(
                        name: "FK_VISITE_LOGEMENT_id_logement",
                        column: x => x.id_logement,
                        principalTable: "LOGEMENT",
                        principalColumn: "id_logement",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VISITE_PROSPECT_id_prospect",
                        column: x => x.id_prospect,
                        principalTable: "PROSPECT",
                        principalColumn: "id_prospect",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VISITE_UTILISATEUR_id_agent",
                        column: x => x.id_agent,
                        principalTable: "UTILISATEUR",
                        principalColumn: "id_utilisateur",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "COMMUNICATION",
                columns: table => new
                {
                    id_communication = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_visite = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    contenu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date_envoi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    statut_envoi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMMUNICATION", x => x.id_communication);
                    table.ForeignKey(
                        name: "FK_COMMUNICATION_VISITE_id_visite",
                        column: x => x.id_visite,
                        principalTable: "VISITE",
                        principalColumn: "id_visite",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RAPPORT_VISITE",
                columns: table => new
                {
                    id_rapport = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_visite = table.Column<int>(type: "int", nullable: false),
                    commentaire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    niveau_interet = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    prospect_present = table.Column<bool>(type: "bit", nullable: false),
                    date_creation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RAPPORT_VISITE", x => x.id_rapport);
                    table.ForeignKey(
                        name: "FK_RAPPORT_VISITE_VISITE_id_visite",
                        column: x => x.id_visite,
                        principalTable: "VISITE",
                        principalColumn: "id_visite",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_COMMUNICATION_id_visite",
                table: "COMMUNICATION",
                column: "id_visite");

            migrationBuilder.CreateIndex(
                name: "IX_DISPONIBILITE_id_utilisateur",
                table: "DISPONIBILITE",
                column: "id_utilisateur");

            migrationBuilder.CreateIndex(
                name: "IX_PHOTO_id_logement",
                table: "PHOTO",
                column: "id_logement");

            migrationBuilder.CreateIndex(
                name: "IX_RAPPORT_VISITE_id_visite",
                table: "RAPPORT_VISITE",
                column: "id_visite",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UTILISATEUR_courriel",
                table: "UTILISATEUR",
                column: "courriel",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VISITE_id_agent",
                table: "VISITE",
                column: "id_agent");

            migrationBuilder.CreateIndex(
                name: "IX_VISITE_id_logement",
                table: "VISITE",
                column: "id_logement");

            migrationBuilder.CreateIndex(
                name: "IX_VISITE_id_prospect",
                table: "VISITE",
                column: "id_prospect");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "COMMUNICATION");

            migrationBuilder.DropTable(
                name: "DISPONIBILITE");

            migrationBuilder.DropTable(
                name: "PHOTO");

            migrationBuilder.DropTable(
                name: "RAPPORT_VISITE");

            migrationBuilder.DropTable(
                name: "VISITE");

            migrationBuilder.DropTable(
                name: "LOGEMENT");

            migrationBuilder.DropTable(
                name: "PROSPECT");

            migrationBuilder.DropTable(
                name: "UTILISATEUR");
        }
    }
}
