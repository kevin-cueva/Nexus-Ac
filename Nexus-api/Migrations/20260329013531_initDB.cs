using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus_api.Migrations
{
    /// <inheritdoc />
    public partial class initDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "nexus");

            migrationBuilder.CreateTable(
                name: "RolTeam",
                schema: "nexus",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolTeam", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sectors",
                schema: "nexus",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sectors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "nexus",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                schema: "nexus",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    webPage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    idSector = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.id);
                    table.ForeignKey(
                        name: "FK_clients_sectors_idSector",
                        column: x => x.idSector,
                        principalSchema: "nexus",
                        principalTable: "sectors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cases",
                schema: "nexus",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    idClient = table.Column<int>(type: "int", nullable: false),
                    country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    year = table.Column<DateTime>(type: "datetime2", nullable: false),
                    duration = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    idSector = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cases", x => x.id);
                    table.ForeignKey(
                        name: "FK_cases_clients_idClient",
                        column: x => x.idClient,
                        principalSchema: "nexus",
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cases_sectors_idSector",
                        column: x => x.idSector,
                        principalSchema: "nexus",
                        principalTable: "sectors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Team",
                schema: "nexus",
                columns: table => new
                {
                    idCase = table.Column<int>(type: "int", nullable: false),
                    idUser = table.Column<int>(type: "int", nullable: false),
                    idRolTeam = table.Column<int>(type: "int", nullable: false),
                    id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team", x => new { x.idCase, x.idUser });
                    table.ForeignKey(
                        name: "FK_Team_RolTeam_idRolTeam",
                        column: x => x.idRolTeam,
                        principalSchema: "nexus",
                        principalTable: "RolTeam",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Team_cases_idCase",
                        column: x => x.idCase,
                        principalSchema: "nexus",
                        principalTable: "cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Team_users_idUser",
                        column: x => x.idUser,
                        principalSchema: "nexus",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cases_idClient",
                schema: "nexus",
                table: "cases",
                column: "idClient");

            migrationBuilder.CreateIndex(
                name: "IX_cases_idSector",
                schema: "nexus",
                table: "cases",
                column: "idSector");

            migrationBuilder.CreateIndex(
                name: "IX_clients_idSector",
                schema: "nexus",
                table: "clients",
                column: "idSector");

            migrationBuilder.CreateIndex(
                name: "IX_Team_idRolTeam",
                schema: "nexus",
                table: "Team",
                column: "idRolTeam");

            migrationBuilder.CreateIndex(
                name: "IX_Team_idUser",
                schema: "nexus",
                table: "Team",
                column: "idUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Team",
                schema: "nexus");

            migrationBuilder.DropTable(
                name: "RolTeam",
                schema: "nexus");

            migrationBuilder.DropTable(
                name: "cases",
                schema: "nexus");

            migrationBuilder.DropTable(
                name: "users",
                schema: "nexus");

            migrationBuilder.DropTable(
                name: "clients",
                schema: "nexus");

            migrationBuilder.DropTable(
                name: "sectors",
                schema: "nexus");
        }
    }
}
