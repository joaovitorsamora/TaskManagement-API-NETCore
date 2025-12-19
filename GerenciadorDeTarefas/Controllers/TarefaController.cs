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


        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TarefaDTO>>> GetAllAsync()
        {
            
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var loggedInUserId))
                return Unauthorized("Usuário não autenticado ou token inválido.");

           
            var tarefas = await _repository.GetAllAsync();
            var tarefasDoUsuario = tarefas.Where(t => t.UsuarioId == loggedInUserId);

            var dtos = tarefasDoUsuario.Select(t => new TarefaDTO
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

            return Ok(dtos);
        }

        [HttpGet("{id}", Name = "GetTarefa")]
        public async Task<ActionResult<TarefaDTO>> GetByIdAsync(int id, [FromQuery] int? usuarioId = null)
        {
            var tarefa = await _repository.GetByIdAsync(id);
            if (tarefa == null)
                return NotFound();

            if (usuarioId.HasValue && tarefa.UsuarioId != usuarioId.Value)
                return Forbid();

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
                Tags = tarefa.Tags?.Select(tag => tag.Nome).ToList()
            });
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<TarefaDTO>> PostAsync(
    [FromBody] TarefaDTO dto,
    [FromServices] IProjetoRepository projetoRepository,
    [FromServices] ITagRepository tagRepository,
    [FromServices] IUsuarioRepository usuarioRepository)
        {
            
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var loggedInUserId))
                return Unauthorized("Usuário não autenticado ou token inválido.");

            
            var usuario = await usuarioRepository.GetByIdAsync(loggedInUserId);
            if (usuario == null)
                return NotFound("Usuário logado não encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Titulo))
                return BadRequest("Título é obrigatório.");

            if (!dto.StatusTarefa.HasValue)
                return BadRequest("StatusTarefa é obrigatório.");

            if (!dto.PrioridadeTarefa.HasValue)
                return BadRequest("PrioridadeTarefa é obrigatória.");

          
            ProjetoModel projeto = null;

            if (dto.ProjetoId.HasValue)
            {
                projeto = await projetoRepository.GetByIdAsync(dto.ProjetoId.Value);
                if (projeto == null)
                    return BadRequest("ProjetoId inválido.");
            }
            else if (!string.IsNullOrWhiteSpace(dto.ProjetoNome))
            {
                projeto = (await projetoRepository.GetAllAsync())
                    .FirstOrDefault(p =>
                        p.UsuarioId == loggedInUserId &&
                        p.Nome.Trim().Equals(dto.ProjetoNome.Trim(), StringComparison.OrdinalIgnoreCase));

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
            }

          
            var tags = new List<TagModel>();

            if (dto.Tags != null && dto.Tags.Any())
            {
                var todasTags = await tagRepository.GetAllAsync();

                foreach (var tagNome in dto.Tags)
                {
                    if (string.IsNullOrWhiteSpace(tagNome)) continue;

                    var tag = todasTags.FirstOrDefault(t =>
                        t.Nome.Trim().Equals(tagNome.Trim(), StringComparison.OrdinalIgnoreCase));

                    if (tag == null)
                    {
                        tag = new TagModel { Nome = tagNome.Trim() };
                        await tagRepository.PostAsync(tag);
                        await tagRepository.SaveChangesAsync();
                    }

                    tags.Add(tag);
                }
            }

            var tarefa = new TarefaModel
            {
                Titulo = dto.Titulo.Trim(),
                DataCriacao = dto.DataCriacao == default ? DateTime.UtcNow : dto.DataCriacao,
                StatusTarefa = dto.StatusTarefa.Value,
                PrioridadeTarefa = dto.PrioridadeTarefa.Value,
                ProjetoId = projeto?.Id,
                UsuarioId = loggedInUserId,
                Tags = tags
            };

           
            await _repository.PostAsync(tarefa);
            await _repository.SaveChangesAsync();

          
            var dtoResult = new TarefaDTO
            {
                Id = tarefa.Id,
                Titulo = tarefa.Titulo,
                DataCriacao = tarefa.DataCriacao,
                ProjetoId = tarefa.ProjetoId,
                UsuarioId = tarefa.UsuarioId,
                ProjetoNome = projeto?.Nome,
                StatusTarefa = tarefa.StatusTarefa,
                PrioridadeTarefa = tarefa.PrioridadeTarefa,
                Tags = tarefa.Tags.Select(t => t.Nome).ToList()
            };

            return CreatedAtRoute("GetTarefa", new { id = tarefa.Id }, dtoResult);
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

            if (!string.IsNullOrWhiteSpace(dto.Titulo))
                tarefaExistente.Titulo = dto.Titulo.Trim();

            if (dto.StatusTarefa.HasValue)
                tarefaExistente.StatusTarefa = dto.StatusTarefa.Value;

            if (dto.PrioridadeTarefa.HasValue)
                tarefaExistente.PrioridadeTarefa = dto.PrioridadeTarefa.Value;

            tarefaExistente.UsuarioId = loggedInUserId;

          
            if (dto.ProjetoId.HasValue)
            {
                tarefaExistente.ProjetoId = dto.ProjetoId.Value;
            }
            else if (!string.IsNullOrWhiteSpace(dto.ProjetoNome))
            {
                var projeto = (await projetoRepository.GetAllAsync())
                    .FirstOrDefault(p =>
                        p.Nome.Trim().Equals(dto.ProjetoNome.Trim(), StringComparison.OrdinalIgnoreCase)
                        && p.UsuarioId == loggedInUserId);

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

          
            if (dto.Tags != null)
            {
                var todasTags = await tagRepository.GetAllAsync();
                var tags = new List<TagModel>();

                foreach (var tagNome in dto.Tags)
                {
                    if (string.IsNullOrWhiteSpace(tagNome)) continue;

                    var tag = todasTags.FirstOrDefault(t =>
                        t.Nome.Trim().Equals(tagNome.Trim(), StringComparison.OrdinalIgnoreCase));

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


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
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