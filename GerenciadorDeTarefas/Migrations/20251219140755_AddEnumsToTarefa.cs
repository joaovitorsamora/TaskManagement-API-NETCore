using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerenciadorDeTarefas.Migrations
{
    /// <inheritdoc />
    public partial class AddEnumsToTarefa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:prioridade", "todas,alta,media,baixa")
                .Annotation("Npgsql:Enum:status", "aberta,concluida");

            migrationBuilder.AlterColumn<int>(
                name: "StatusTarefa",
                table: "Tarefas",
                type: "status_enum",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PrioridadeTarefa",
                table: "Tarefas",
                type: "prioridade_enum",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:prioridade", "todas,alta,media,baixa")
                .OldAnnotation("Npgsql:Enum:status", "aberta,concluida");

            migrationBuilder.AlterColumn<int>(
                name: "StatusTarefa",
                table: "Tarefas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "status_enum",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PrioridadeTarefa",
                table: "Tarefas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "prioridade_enum",
                oldNullable: true);
        }
    }
}
