using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChordFinderAPI.Migrations
{
    /// <inheritdoc />
    public partial class SistemaMusical : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Acordes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acordes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Escalas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Escalas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AcordeEscala",
                columns: table => new
                {
                    AcordesId = table.Column<int>(type: "int", nullable: false),
                    EscalasId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcordeEscala", x => new { x.AcordesId, x.EscalasId });
                    table.ForeignKey(
                        name: "FK_AcordeEscala_Acordes_AcordesId",
                        column: x => x.AcordesId,
                        principalTable: "Acordes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AcordeEscala_Escalas_EscalasId",
                        column: x => x.EscalasId,
                        principalTable: "Escalas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcordeEscala_EscalasId",
                table: "AcordeEscala",
                column: "EscalasId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcordeEscala");

            migrationBuilder.DropTable(
                name: "Acordes");

            migrationBuilder.DropTable(
                name: "Escalas");
        }
    }
}
