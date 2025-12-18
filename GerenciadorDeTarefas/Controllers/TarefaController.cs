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

            int usuarioId = loggedInUserId;

          
            if (!Enum.TryParse<Status>(dto.StatusTarefa.ToString(), true, out var status))
                return BadRequest("StatusTarefa inválido.");

            if (!Enum.TryParse<Prioridade>(dto.PrioridadeTarefa.ToString(), true, out var prioridade))
                return BadRequest("PrioridadeTarefa inválida.");

            
            ProjetoModel projeto = null;
            if (!string.IsNullOrWhiteSpace(dto.ProjetoNome))
            {
                projeto = (await projetoRepository.GetAllAsync())
                    .FirstOrDefault(p => p.UsuarioId == usuarioId &&
                                            p.Nome.Trim().Equals(dto.ProjetoNome.Trim(), StringComparison.OrdinalIgnoreCase));

                if (projeto == null)
                {
                    var usuario = await usuarioRepository.GetByIdAsync(usuarioId);
                    if (usuario == null) return NotFound("Usuário logado não encontrado.");

                    projeto = new ProjetoModel
                    {
                        Nome = dto.ProjetoNome.Trim(),
                        UsuarioId = usuarioId,
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
                    var tag = todasTags.FirstOrDefault(t =>
                        string.Equals(t.Nome.Trim(), tagNome.Trim(), StringComparison.OrdinalIgnoreCase));

                    if (tag == null)
                    {
                        
                        tag = new TagModel { Nome = tagNome.Trim() };
                        await tagRepository.PostAsync(tag);
                        }

                    tags.Add(tag);
                }
            }

            var tarefa = new TarefaModel
            {
                Titulo = dto.Titulo.Trim(),
                DataCriacao = dto.DataCriacao,
                StatusTarefa = status,
                PrioridadeTarefa = prioridade,
                ProjetoId = projeto.Id,
                UsuarioId = usuarioId,
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
                StatusTarefa = status,
                PrioridadeTarefa = prioridade,
                Tags = tarefa.Tags.Select(t => t.Nome).ToList()
            };

            return CreatedAtRoute("GetTarefa", new { id = tarefa.Id, usuarioId }, dtoResult);
        }


      
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(
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
                return NotFound("Usuário logado não encontrado na base de dados.");

           
            ProjetoModel projeto = null;
            if (!string.IsNullOrWhiteSpace(dto.ProjetoNome))
            {
                projeto = (await projetoRepository.GetAllAsync())
                    .FirstOrDefault(p => p.Nome.Trim().Equals(dto.ProjetoNome.Trim(), StringComparison.OrdinalIgnoreCase)
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
            }


          
            if (!Enum.TryParse<Prioridade>(dto.PrioridadeTarefa.ToString(), true, out var prioridade))
                return BadRequest("PrioridadeTarefa inválida.");



            
            tarefaExistente.Titulo = dto.Titulo.Trim();
            tarefaExistente.ProjetoId = projeto.Id;
            tarefaExistente.UsuarioId = loggedInUserId; 
            tarefaExistente.PrioridadeTarefa = prioridade;
            tarefaExistente.StatusTarefa = dto.StatusTarefa;

            
            if (dto.Tags != null && dto.Tags.Any())
            {
                var todasTags = await tagRepository.GetAllAsync();
                var tags = new List<TagModel>();
                foreach (var tagNome in dto.Tags)
                {
                    var tag = todasTags.FirstOrDefault(t =>
                        string.Equals(t.Nome.Trim(), tagNome.Trim(), StringComparison.OrdinalIgnoreCase));

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
            else
            {
                if (tarefaExistente.Tags != null)
                {
                    tarefaExistente.Tags.Clear();
                }
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