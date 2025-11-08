using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositiries
{
    public class TaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<TaskItem>> GetAllAsync() => await _context.Tasks.ToListAsync();

        public async Task<TaskItem?> GetByIdAsync(int id) => await _context.Tasks.FindAsync(id);

        public async Task<TaskItem> CreateAsync(TaskItem task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem?> UpdateAsync(int id, TaskItem updateTask)
        {
            var existingTask = await _context.Tasks.FindAsync(id);
            if (existingTask == null) return null;
            
            existingTask.Title = updateTask.Title;
            existingTask.Description = updateTask.Description;
            existingTask.DueDate = updateTask.DueDate;
            existingTask.IsCompleted = updateTask.IsCompleted;
            existingTask.Priority = updateTask.Priority;

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
    }
}
