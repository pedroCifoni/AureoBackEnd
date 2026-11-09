using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aureo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPapelUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Papel",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Papel",
                table: "Usuarios");
        }
    }
}
