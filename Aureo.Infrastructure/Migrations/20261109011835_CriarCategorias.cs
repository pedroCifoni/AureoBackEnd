using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aureo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriarCategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Cor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Icone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categorias_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_UsuarioId_Nome_Tipo",
                table: "Categorias",
                columns: new[] { "UsuarioId", "Nome", "Tipo" },
                unique: true);

            // Coluna nasce anulável para receber os dados convertidos antes de virar NOT NULL.
            migrationBuilder.AddColumn<Guid>(
                name: "CategoriaId",
                table: "Transacoes",
                type: "uuid",
                nullable: true);

            // Uma categoria por (usuário, nome sem diferenciar caixa, tipo) a partir do texto antigo.
            migrationBuilder.Sql(@"
                INSERT INTO ""Categorias"" (""Id"", ""Nome"", ""Tipo"", ""Cor"", ""Icone"", ""UsuarioId"")
                SELECT gen_random_uuid(), MIN(LEFT(TRIM(""Categoria""), 50)), ""Tipo"", '#6B7280', NULL, ""UsuarioId""
                FROM ""Transacoes""
                GROUP BY ""UsuarioId"", LOWER(LEFT(TRIM(""Categoria""), 50)), ""Tipo"";");

            migrationBuilder.Sql(@"
                UPDATE ""Transacoes"" t
                SET ""CategoriaId"" = c.""Id""
                FROM ""Categorias"" c
                WHERE c.""UsuarioId"" = t.""UsuarioId""
                  AND c.""Tipo"" = t.""Tipo""
                  AND LOWER(c.""Nome"") = LOWER(LEFT(TRIM(t.""Categoria""), 50));");

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoriaId",
                table: "Transacoes",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Transacoes");

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_CategoriaId",
                table: "Transacoes",
                column: "CategoriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transacoes_Categorias_CategoriaId",
                table: "Transacoes",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transacoes_Categorias_CategoriaId",
                table: "Transacoes");

            migrationBuilder.DropIndex(
                name: "IX_Transacoes_CategoriaId",
                table: "Transacoes");

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Transacoes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Transacoes"" t
                SET ""Categoria"" = c.""Nome""
                FROM ""Categorias"" c
                WHERE c.""Id"" = t.""CategoriaId"";");

            migrationBuilder.AlterColumn<string>(
                name: "Categoria",
                table: "Transacoes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "CategoriaId",
                table: "Transacoes");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
