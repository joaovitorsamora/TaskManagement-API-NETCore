using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerenciadorDeTarefas.Migrations
{
    /// <inheritdoc />
    public partial class SyncEnumsWithPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:prioridade_enum.prioridade", "todas,alta,media,baixa")
                .OldAnnotation("Npgsql:Enum:status_enum.status", "aberta,concluida");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:prioridade_enum.prioridade", "todas,alta,media,baixa")
                .Annotation("Npgsql:Enum:status_enum.status", "aberta,concluida");
        }
    }
}
