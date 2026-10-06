using GerenciadorDeTarefas.DTOs;
using GerenciadorDeTarefas.Models;
using GerenciadorDeTarefas.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Razor.TagHelpers;


namespace GerenciadorDeTarefas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TagController : ControllerBase
    {
        private readonly ITagRepository _repository;
        public TagController(ITagRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllAsync()
        {
            var tags = await _repository.GetAllAsync();
            return Ok(tags);
        }
       
        [HttpGet("{id}")]
        public async Task<ActionResult<TagModel>> GetByIdAsync(int id)
        {
            var tag = await _repository.GetByIdAsync(id);
            if (tag == null) return NotFound();
            return Ok(tag);
        }
       
        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody] TagDTO dto)
        {
            var tag = new TagModel
            {
                Nome = dto.Nome
            };

            await _repository.PostAsync(tag);
            await _repository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByIdAsync), new { id = tag.Id }, new TagDTO
            {

                Id = tag.Id,
                Nome = tag.Nome
            });
        }
        
        [HttpGet("{tagId}/tarefas")]

        public async Task<ActionResult> GetAllTarefaAsync(int tagId)
        {
            var tarefas = await _repository.GetAllTarefaAsync(tagId);
            return Ok(tarefas);
        }
    }
}
