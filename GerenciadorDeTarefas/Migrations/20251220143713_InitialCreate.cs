using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GerenciadorDeTarefas.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:prioridade_enum.prioridade", "todas,alta,media,baixa")
                .Annotation("Npgsql:Enum:status_enum.status", "aberta,concluida");

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    SenhaHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projetos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioNome = table.Column<string>(type: "text", nullable: true),
                    ProjetoId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioModelId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projetos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projetos_Usuarios_UsuarioModelId",
                        column: x => x.UsuarioModelId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: true),
                    ProjetoModelId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_Projetos_ProjetoModelId",
                        column: x => x.ProjetoModelId,
                        principalTable: "Projetos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tarefas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "text", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StatusTarefa = table.Column<int>(type: "status_enum", nullable: false),
                    PrioridadeTarefa = table.Column<int>(type: "prioridade_enum", nullable: false),
                    ProjetoId = table.Column<int>(type: "integer", nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tarefas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tarefas_Projetos_ProjetoId",
                        column: x => x.ProjetoId,
                        principalTable: "Projetos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tarefas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagModelTarefaModel",
                columns: table => new
                {
                    TagsId = table.Column<int>(type: "integer", nullable: false),
                    TarefasId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagModelTarefaModel", x => new { x.TagsId, x.TarefasId });
                    table.ForeignKey(
                        name: "FK_TagModelTarefaModel_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagModelTarefaModel_Tarefas_TarefasId",
                        column: x => x.TarefasId,
                        principalTable: "Tarefas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projetos_UsuarioModelId",
                table: "Projetos",
                column: "UsuarioModelId");

            migrationBuilder.CreateIndex(
                name: "IX_TagModelTarefaModel_TarefasId",
                table: "TagModelTarefaModel",
                column: "TarefasId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ProjetoModelId",
                table: "Tags",
                column: "ProjetoModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefas_ProjetoId",
                table: "Tarefas",
                column: "ProjetoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefas_UsuarioId",
                table: "Tarefas",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagModelTarefaModel");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Tarefas");

            migrationBuilder.DropTable(
                name: "Projetos");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
