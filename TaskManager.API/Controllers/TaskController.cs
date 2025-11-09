using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger<TaskController> _logger;

        public TaskController(ITaskRepository taskRepository, ILogger<TaskController> logger)
        {
            _taskRepository = taskRepository;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetById(int id)
        {
            _logger.LogInformation("Getting task by ID: {TaskId}", id);
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return NotFound();

            return Ok(task);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
        {
            _logger.LogInformation("GET /api/task - Getting all tasks");

            try
            {
                var tasks = await _taskRepository.GetAllAsync();
                _logger.LogInformation("GET /api/task - Retrieved {TaskCount} tasks", tasks.Count());
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET /api/task - Error retrieving tasks");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> Create([FromBody] TaskItem task)
        {
            _logger.LogInformation("Creating new task: {TaskTitle}", task.Title);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("POST /api/task - Validation failed: {ValidationErrors}",
                    string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdTask = await _taskRepository.CreateAsync(task);
            return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, createdTask);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TaskItem>> UpdateTask(int id, [FromBody] TaskItem task)
        {
            _logger.LogInformation("Updating task ID: {TaskId}", id);

            if (id != task.Id ) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updatedTask = await _taskRepository.UpdateAsync(task);

            return Ok(updatedTask);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting task ID: {TaskId}", id);

            var result = await _taskRepository.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
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

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<TaskItem>>> GetPaged([FromQuery] TaskQueryParameters parameters)
        {
            _logger.LogInformation("GET /api/task/paged - Page: {Page}, Size: {Size}", 
                parameters.PageNumber, parameters.PageSize);

            try
            {
                var result = await _taskRepository.GetPagedAsync(parameters);
                _logger.LogInformation("GET /api/task/paged - Returned {Count} of {Total} tasks",
                    result.Items.Count(), result.TotalCount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET /api/task/paged - Error retrieving paged tasks");
                return StatusCode(500, "Internal server error");
            }

        }
    }
}
