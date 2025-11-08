using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;
using TaskManager.Infrastructure.Repositiries;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        //private readonly TaskRepository _taskRepository;
        private readonly ITaskRepository _taskRepository;

        public TaskController(ITaskRepository taskRepository)
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
        public async Task<ActionResult<TaskItem>> UpdateTask(int id, [FromBody] TaskItem updatedTask)
        {
            if (id != updatedTask.Id)
                return BadRequest();

            var existingTask = await _taskRepository.GetByIdAsync(id);
            if (existingTask == null)
                return NotFound();

            var result = await _taskRepository.UpdateAsync(updatedTask);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _taskRepository.DeleteAsync(id);
            if (!result) return NotFound();
            
            return Ok(result);
        }

        [HttpPatch("{id}/toggle")]
        public async Task<ActionResult<TaskItem>> ToggleTask(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                return NotFound();

            task.IsCompleted = !task.IsCompleted;
            var result = await _taskRepository.UpdateAsync(task);
            return Ok(result);
        }
    }
}
