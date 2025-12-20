using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerenciadorDeTarefas.Migrations
{
    public partial class FixEnumNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1️⃣ Corrige dados existentes (NULL → valor válido do enum)
            migrationBuilder.Sql("""
                UPDATE "Tarefas"
                SET "StatusTarefa" = 'Aberta'
                WHERE "StatusTarefa" IS NULL;

                UPDATE "Tarefas"
                SET "PrioridadeTarefa" = 'Todas'
                WHERE "PrioridadeTarefa" IS NULL;
            """);

            // 2️⃣ Torna as colunas NOT NULL (SEM default numérico)
            migrationBuilder.AlterColumn<GerenciadorDeTarefas.Models.Status>(
                name: "StatusTarefa",
                table: "Tarefas",
                type: "status_enum",
                nullable: false,
                oldClrType: typeof(GerenciadorDeTarefas.Models.Status),
                oldType: "status_enum",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<GerenciadorDeTarefas.Models.Prioridade>(
                name: "PrioridadeTarefa",
                table: "Tarefas",
                type: "prioridade_enum",
                nullable: false,
                oldClrType: typeof(GerenciadorDeTarefas.Models.Prioridade),
                oldType: "prioridade_enum",
                oldNullable: true
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<GerenciadorDeTarefas.Models.Status>(
                name: "StatusTarefa",
                table: "Tarefas",
                type: "status_enum",
                nullable: true,
                oldClrType: typeof(GerenciadorDeTarefas.Models.Status),
                oldType: "status_enum"
            );

            migrationBuilder.AlterColumn<GerenciadorDeTarefas.Models.Prioridade>(
                name: "PrioridadeTarefa",
                table: "Tarefas",
                type: "prioridade_enum",
                nullable: true,
                oldClrType: typeof(GerenciadorDeTarefas.Models.Prioridade),
                oldType: "prioridade_enum"
            );
        }
    }
}
