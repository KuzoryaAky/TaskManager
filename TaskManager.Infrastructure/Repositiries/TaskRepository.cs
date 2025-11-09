using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositiries
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<TaskItem>> GetAllAsync() => await _context.Tasks.ToListAsync();

        public async Task<TaskItem> GetByIdAsync(int id) => await _context.Tasks.FindAsync(id);

        public async Task<TaskItem> AddAsync(TaskItem task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem> CreateAsync(TaskItem task)
        {
            if (string.IsNullOrEmpty(task.Title))
                task.Title = task.Description ?? "New Task";

            if (task.CreatedAt == default)
                task.CreatedAt = DateTime.UtcNow;

            if (task.Priority == 0)
                task.Priority = 1;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }


        public async Task<TaskItem> UpdateAsync(TaskItem task)
        {
            var existingTask = await _context.Tasks.FindAsync(task.Id);
            if (existingTask == null)
                throw new ArgumentException($"Task with id {task.Id} not found");

            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.IsCompleted = task.IsCompleted;
            existingTask.Priority = task.Priority;
            existingTask.DueDate = task.DueDate;
            existingTask.CreatedAt = task.CreatedAt;

            await _context.SaveChangesAsync();
            return existingTask;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResult<TaskItem>> GetPagedAsync(TaskQueryParameters parameters)
        {
            var query = _context.Tasks.AsQueryable();

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<TaskItem> 
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }
    }
}
