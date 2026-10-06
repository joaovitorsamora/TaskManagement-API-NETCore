using GerenciadorDeTarefas.DTOs;
using GerenciadorDeTarefas.Models;
using GerenciadorDeTarefas.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 
using System.Security.Claims; 
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GerenciadorDeTarefas.Controllers
{
    [Route("api/usuarios")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioRepository _repository;
        private readonly IWebHostEnvironment _env; 

        public UsuarioController(IUsuarioRepository repository, IWebHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDTO>>> GetUsuario()
        {
            if (!_env.IsDevelopment())
            {
                return StatusCode(StatusCodes.Status403Forbidden, "A listagem completa de usuários está disponível apenas em ambiente de desenvolvimento.");
            }

            var usuarios = await _repository.GetAllAsync();
            var dtos = usuarios.Select(u => new UsuarioResponseDTO
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email
            });
            return Ok(dtos);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UsuarioResponseDTO>> GetMe()
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var loggedInUserId))
                return Unauthorized("Usuário não autenticado.");

            var usuario = await _repository.GetByIdAsync(loggedInUserId);
            if (usuario == null) return NotFound();

            var dto = new UsuarioResponseDTO
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email
            };

            return Ok(dto);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioResponseDTO>> GetUsuarioById(int id)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var loggedInUserId))
                return Unauthorized();

            if (id != loggedInUserId && !_env.IsDevelopment())
                return StatusCode(StatusCodes.Status403Forbidden, "Você só pode ver os dados do seu próprio usuário.");

            var usuario = await _repository.GetByIdAsync(id);
            if (usuario == null) return NotFound();

            return Ok(new UsuarioResponseDTO { Id = usuario.Id, Nome = usuario.Nome, Email = usuario.Email });
        }
    }
}
