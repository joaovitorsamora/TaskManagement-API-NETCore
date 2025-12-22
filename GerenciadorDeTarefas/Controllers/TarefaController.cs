using GerenciadorDeTarefas.DTOs;
using GerenciadorDeTarefas.Models;
using GerenciadorDeTarefas.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GerenciadorDeTarefas.Controllers
{
    [Route("api/tarefas")]
    [ApiController]
    public class TarefaController : ControllerBase
    {
        private readonly ITarefaRepository _repository;

        public TarefaController(ITarefaRepository repository)
        {
            _repository = repository;
        }

        // ========================= GET ALL =========================
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TarefaDTO>>> GetAllAsync()
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                return Unauthorized();

            var tarefas = await _repository.GetAllAsync();

            var result = tarefas
                .Where(t => t.UsuarioId == userId)
                .Select(t => new TarefaDTO
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    DataCriacao = t.DataCriacao,
                    ProjetoId = t.ProjetoId,
                    UsuarioId = t.UsuarioId,
                    ProjetoNome = t.Projeto?.Nome,
                    StatusTarefa = t.StatusTarefa,
                    PrioridadeTarefa = t.PrioridadeTarefa,
                    Tags = t.Tags?.Select(tag => tag.Nome).ToList()
                });

            return Ok(result);
        }

        // ========================= GET BY ID =========================
        [HttpGet("{id}", Name = "GetTarefa")]
        public async Task<ActionResult<TarefaDTO>> GetByIdAsync(int id)
        {
            var tarefa = await _repository.GetByIdAsync(id);
            if (tarefa == null)
                return NotFound();

            return Ok(new TarefaDTO
            {
                Id = tarefa.Id,
                Titulo = tarefa.Titulo,
                DataCriacao = tarefa.DataCriacao,
                ProjetoId = tarefa.ProjetoId,
                UsuarioId = tarefa.UsuarioId,
                ProjetoNome = tarefa.Projeto?.Nome,
                StatusTarefa = tarefa.StatusTarefa,
                PrioridadeTarefa = tarefa.PrioridadeTarefa,
                Tags = tarefa.Tags?.Select(t => t.Nome).ToList()
            });
        }

        // ========================= POST =========================
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<TarefaDTO>> PostAsync(
            [FromBody] TarefaDTO dto,
            [FromServices] IProjetoRepository projetoRepository,
            [FromServices] ITagRepository tagRepository,
            [FromServices] IUsuarioRepository usuarioRepository)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.Titulo))
                return BadRequest("Título é obrigatório.");

            if (!dto.StatusTarefa.HasValue || !dto.PrioridadeTarefa.HasValue)
                return BadRequest("Status e Prioridade são obrigatórios.");

            var usuario = await usuarioRepository.GetByIdAsync(userId);
            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            // ---------- Projeto ----------
            ProjetoModel projeto = null;

            if (dto.ProjetoId.HasValue)
            {
                projeto = await projetoRepository.GetByIdAsync(dto.ProjetoId.Value);
                if (projeto == null)
                    return BadRequest("Projeto inválido.");
            }
            else if (!string.IsNullOrWhiteSpace(dto.ProjetoNome))
            {
                projeto = (await projetoRepository.GetAllAsync())
                    .FirstOrDefault(p =>
                        p.UsuarioId == userId &&
                        p.Nome.Equals(dto.ProjetoNome.Trim(), StringComparison.OrdinalIgnoreCase));

                if (projeto == null)
                {
                    projeto = new ProjetoModel
                    {
                        Nome = dto.ProjetoNome.Trim(),
                        UsuarioId = userId,
                        UsuarioNome = usuario.Nome
                    };

                    await projetoRepository.PostAsync(projeto);
                    await projetoRepository.SaveChangesAsync();
                }
            }

            // ---------- Tags ----------
            var tags = new List<TagModel>();
            if (dto.Tags?.Any() == true)
            {
                var todasTags = await tagRepository.GetAllAsync();

                foreach (var nome in dto.Tags.Where(t => !string.IsNullOrWhiteSpace(t)))
                {
                    var tag = todasTags.FirstOrDefault(t =>
                        t.Nome.Equals(nome.Trim(), StringComparison.OrdinalIgnoreCase));

                    if (tag == null)
                    {
                        tag = new TagModel { Nome = nome.Trim() };
                        await tagRepository.PostAsync(tag);
                        await tagRepository.SaveChangesAsync();
                    }

                    tags.Add(tag);
                }
            }

            // ---------- Tarefa ----------
            var tarefa = new TarefaModel
            {
                Titulo = dto.Titulo.Trim(),
                DataCriacao = DateTime.UtcNow, // 🔥 FIX DEFINITIVO
                StatusTarefa = dto.StatusTarefa.Value,
                PrioridadeTarefa = dto.PrioridadeTarefa.Value,
                ProjetoId = projeto?.Id,
                UsuarioId = userId,
                Tags = tags
            };

            await _repository.PostAsync(tarefa);
            await _repository.SaveChangesAsync();

            return CreatedAtRoute("GetTarefa", new { id = tarefa.Id }, new TarefaDTO
            {
                Id = tarefa.Id,
                Titulo = tarefa.Titulo,
                DataCriacao = tarefa.DataCriacao,
                ProjetoId = tarefa.ProjetoId,
                UsuarioId = tarefa.UsuarioId,
                ProjetoNome = projeto?.Nome,
                StatusTarefa = tarefa.StatusTarefa,
                PrioridadeTarefa = tarefa.PrioridadeTarefa,
                Tags = tags.Select(t => t.Nome).ToList()
            });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(
    int id,
    [FromBody] TarefaDTO dto,
    [FromServices] ITagRepository tagRepository,
    [FromServices] IProjetoRepository projetoRepository,
    [FromServices] IUsuarioRepository usuarioRepository)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var loggedInUserId))
                return Unauthorized("Usuário não autenticado ou token inválido.");

            var tarefaExistente = await _repository.GetByIdAsync(id);
            if (tarefaExistente == null)
                return NotFound("Tarefa não encontrada.");

            if (tarefaExistente.UsuarioId != loggedInUserId)
                return Forbid("Você só pode editar suas próprias tarefas.");

            var usuario = await usuarioRepository.GetByIdAsync(loggedInUserId);
            if (usuario == null)
                return NotFound("Usuário logado não encontrado.");

            // =========================
            // CAMPOS SIMPLES
            // =========================
            if (!string.IsNullOrWhiteSpace(dto.Titulo))
                tarefaExistente.Titulo = dto.Titulo.Trim();

            if (dto.StatusTarefa.HasValue)
                tarefaExistente.StatusTarefa = dto.StatusTarefa.Value;

            if (dto.PrioridadeTarefa.HasValue)
                tarefaExistente.PrioridadeTarefa = dto.PrioridadeTarefa.Value;

            // ⚠️ NÃO ALTERAR DataCriacao
            // tarefaExistente.DataCriacao permanece como está (UTC)

            // =========================
            // PROJETO
            // =========================
            if (dto.ProjetoId.HasValue)
            {
                tarefaExistente.ProjetoId = dto.ProjetoId.Value;
            }
            else if (!string.IsNullOrWhiteSpace(dto.ProjetoNome))
            {
                var projeto = (await projetoRepository.GetAllAsync())
                    .FirstOrDefault(p =>
                        p.UsuarioId == loggedInUserId &&
                        p.Nome.Equals(dto.ProjetoNome.Trim(), StringComparison.OrdinalIgnoreCase));

                if (projeto == null)
                {
                    projeto = new ProjetoModel
                    {
                        Nome = dto.ProjetoNome.Trim(),
                        UsuarioId = loggedInUserId,
                        UsuarioNome = usuario.Nome
                    };

                    await projetoRepository.PostAsync(projeto);
                    await projetoRepository.SaveChangesAsync();
                }

                tarefaExistente.ProjetoId = projeto.Id;
            }

            // =========================
            // TAGS
            // =========================
            if (dto.Tags != null)
            {
                var todasTags = await tagRepository.GetAllAsync();
                var tags = new List<TagModel>();

                foreach (var tagNome in dto.Tags)
                {
                    if (string.IsNullOrWhiteSpace(tagNome)) continue;

                    var tag = todasTags.FirstOrDefault(t =>
                        t.Nome.Equals(tagNome.Trim(), StringComparison.OrdinalIgnoreCase));

                    if (tag == null)
                    {
                        tag = new TagModel { Nome = tagNome.Trim() };
                        await tagRepository.PostAsync(tag);
                        await tagRepository.SaveChangesAsync();
                    }

                    tags.Add(tag);
                }

                tarefaExistente.Tags = tags;
            }

            await _repository.UpdateAsync(tarefaExistente);
            await _repository.SaveChangesAsync();

            return NoContent();
        }



        // ========================= DELETE =========================
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var tarefa = await _repository.GetByIdAsync(id);
            if (tarefa == null)
                return NotFound();

            await _repository.DeleteAsync(tarefa);
            await _repository.SaveChangesAsync();

            return NoContent();
        }
    }
}
