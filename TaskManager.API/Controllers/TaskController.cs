using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.Entities;
using TaskManager.Infrastructure.Repositiries;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly TaskRepository _taskRepository;

        public TaskController(TaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<TaskItem>>> GetAll()
        {
            var tasks = await _taskRepository.GetAllAsync();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetById(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return NotFound();

            return Ok(task);
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> Create(TaskItem task)
        {
            var createTask = await _taskRepository.CreateAsync(task);
            return CreatedAtAction(nameof(GetById), new { id = createTask.Id }, createTask);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TaskItem>> Update(int id, TaskItem task)
        {
            if (id != task.Id) return BadRequest("ID in URL does not match ID in body");

            var updateTask = await _taskRepository.UpdateAsync(id, task);
            if (updateTask == null) return NotFound();

            return Ok(updateTask);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _taskRepository.DeleteAsync(id);
            if (!result) return NotFound();
            
            return Ok(result);
        }
    }
}
