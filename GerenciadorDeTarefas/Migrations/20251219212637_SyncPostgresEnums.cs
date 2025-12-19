using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerenciadorDeTarefas.Migrations
{
    /// <inheritdoc />
    public partial class SyncPostgresEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:prioridade_enum", "Todas,Alta,Media,Baixa")
                .Annotation("Npgsql:Enum:status_enum", "Aberta,Concluida")
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:prioridade", "todas,alta,media,baixa")
                .Annotation("Npgsql:Enum:status", "aberta,concluida")
                .OldAnnotation("Npgsql:Enum:prioridade_enum", "Todas,Alta,Media,Baixa")
                .OldAnnotation("Npgsql:Enum:status_enum", "Aberta,Concluida");

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
    }
}
