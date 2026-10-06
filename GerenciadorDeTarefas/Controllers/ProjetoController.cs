using GerenciadorDeTarefas.Models;
using GerenciadorDeTarefas.DTOs;
using GerenciadorDeTarefas.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims; 

namespace GerenciadorDeTarefas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjetoController : ControllerBase
    {
        private readonly IProjetoRepository _repository;
        public ProjetoController(IProjetoRepository repository)
        {
            _repository = repository;
        }

        private int GetLoggedInUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);


        
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjetoDTO>> GetByIdAsync(int id)
        {
            var projeto = await _repository.GetByIdAsync(id);

            if (projeto == null) return NotFound();

            if (projeto.UsuarioId != GetLoggedInUserId()) return StatusCode(StatusCodes.Status403Forbidden, "Você não tem permissão para acessar este projeto.");


            var dto = new ProjetoDTO
            {
                Id = projeto.Id,
                Nome = projeto.Nome,
                UsuarioId = projeto.UsuarioId,
                UsuarioNome = projeto.UsuarioNome,
                Tarefas = projeto.Tarefas?.Select(t => new TarefaDTO
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    DataCriacao = t.DataCriacao,
                    PrioridadeTarefa = t.PrioridadeTarefa,
                    ProjetoId = t.ProjetoId,
                    UsuarioId = t.UsuarioId,
                    Tags = t.Tags?.Select(tag => tag.Nome).ToList()
                }).ToList()
            };

            return Ok(dto);
        }
       
        [HttpGet("projetos")]
        public async Task<ActionResult<List<ProjetoDTO>>> GetAllAsync()
        {
            var projetos = await _repository.GetAllAsync();

            var userId = GetLoggedInUserId();
            var projetosDoUsuario = projetos.Where(p => p.UsuarioId == userId);

            var dtos = projetosDoUsuario.Select(p => new ProjetoDTO
            {
                Id = p.Id,
                Nome = p.Nome,
                UsuarioId = p.UsuarioId,
                UsuarioNome = p.UsuarioNome,
                ProjetoId = p.ProjetoId,
                Tarefas = p.Tarefas?.Select(t => new TarefaDTO
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    DataCriacao = t.DataCriacao,
                    PrioridadeTarefa = t.PrioridadeTarefa,
                    ProjetoId = t.ProjetoId,
                    UsuarioId = t.UsuarioId,
                    ProjetoNome = t.Projeto?.Nome,
                    Tags = t.Tags?.Select(tag => tag.Nome).ToList()
                }).ToList()
            }).ToList();

            return Ok(dtos);
        }
      
        [HttpGet("tarefas")]
        public async Task<ActionResult<List<TarefaDTO>>> GetAllTarefaAsync()
        {
            var tarefas = await _repository.GetAllTarefaAsync();

            var userId = GetLoggedInUserId();
            var tarefasDoUsuario = tarefas.Where(t => t.UsuarioId == userId);

            var dtos = tarefasDoUsuario.Select(t => new TarefaDTO
            {
                Id = t.Id,
                Titulo = t.Titulo,
                DataCriacao = t.DataCriacao,
                ProjetoId = t.ProjetoId,
                UsuarioId = t.UsuarioId,
                PrioridadeTarefa = t.PrioridadeTarefa,
                ProjetoNome = t.Projeto?.Nome,
                Tags = t.Tags.Select(tag => tag.Nome).ToList()
            }).ToList();

            return Ok(dtos);
        }
    }
}