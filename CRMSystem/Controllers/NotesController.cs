using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CRMSystem.Data;
using CRMSystem.Models;

namespace CRMSystem.Controllers
{
    [Authorize]
    public class NotesController : Controller
    {
        private readonly CrmDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotesController(CrmDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Notes/Tasks - Shows pending tasks for the current sales rep
        public async Task<IActionResult> Tasks()
        {
            var notes = await GetTasksAsync(pendingOnly: true);
            ViewData["Title"] = "My Tasks";
            ViewData["ShowPending"] = true;
            await PopulateDropdownsAsync();
            return View("Tasks", notes);
        }

        // GET: Notes/CompletedTasks
        public async Task<IActionResult> CompletedTasks()
        {
            var notes = await GetTasksAsync(pendingOnly: false);
            ViewData["Title"] = "Completed Tasks";
            ViewData["ShowPending"] = false;
            return View("Tasks", notes);
        }

        // POST: Notes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int contactId, string content, bool isNewTodo, int? todoTypeId, int? todoDescId, DateTime? todoDueDate, string? taskUpdate)
        {
            content = content?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Notes content cannot be empty.";
                return RedirectToAction("Details", "Contacts", new { id = contactId });
            }

            if (content.Length > 4000)
            {
                TempData["ErrorMessage"] = "Content cannot exceed 4000 characters.";
                return RedirectToAction("Details", "Contacts", new { id = contactId });
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.ContactId == contactId);

            if (contact == null)
            {
                return NotFound();
            }

            if (!await CanAccessContactAsync(contact))
            {
                return Forbid();
            }

            var note = new Note
            {
                Content = content,
                Date = DateTime.UtcNow,
                IsNewTodo = isNewTodo,
                TodoTypeId = isNewTodo ? todoTypeId : null,
                TodoDescId = isNewTodo ? todoDescId : null,
                TodoDueDate = isNewTodo ? todoDueDate : null,
                TaskStatusId = isNewTodo ? 1 : null, // Pending by default
                TaskUpdate = taskUpdate?.Trim(),
                ContactId = contactId,
                SalesRepId = GetCurrentUserId(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = isNewTodo ? "Task created successfully." : "Note added successfully.";
            return RedirectToAction("Details", "Contacts", new { id = contactId });
        }

        // POST: Notes/MarkCompleted/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkCompleted(int id, string? taskUpdate, string? returnUrl)
        {
            var note = await _context.Notes
                .Include(n => n.Contact)
                .FirstOrDefaultAsync(n => n.NoteId == id);

            if (note == null)
            {
                return NotFound();
            }

            if (!await CanAccessNoteAsync(note))
            {
                return Forbid();
            }

            if (!note.IsNewTodo)
            {
                TempData["ErrorMessage"] = "Only tasks can be marked as completed.";
                return RedirectToAction("Details", "Contacts", new { id = note.ContactId });
            }

            note.TaskStatusId = 2; // Completed
            if (!string.IsNullOrEmpty(taskUpdate))
            {
                note.TaskUpdate = taskUpdate.Trim();
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Task marked as completed.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Details", "Contacts", new { id = note.ContactId });
        }

        // POST: Notes/Reopen/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reopen(int id, string? returnUrl)
        {
            var note = await _context.Notes
                .Include(n => n.Contact)
                .FirstOrDefaultAsync(n => n.NoteId == id);

            if (note == null)
            {
                return NotFound();
            }

            if (!await CanAccessNoteAsync(note))
            {
                return Forbid();
            }

            if (!note.IsNewTodo)
            {
                TempData["ErrorMessage"] = "Only tasks can be reopened.";
                return RedirectToAction("Details", "Contacts", new { id = note.ContactId });
            }

            note.TaskStatusId = 1; // Pending

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Task reopened.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Details", "Contacts", new { id = note.ContactId });
        }

        // POST: Notes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? returnUrl)
        {
            var note = await _context.Notes
                .Include(n => n.Contact)
                .FirstOrDefaultAsync(n => n.NoteId == id);

            if (note == null)
            {
                return NotFound();
            }

            if (!await CanDeleteNoteAsync(note))
            {
                return Forbid();
            }

            int contactId = note.ContactId;

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Deleted successfully.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Details", "Contacts", new { id = contactId });
        }

        // POST: Notes/UpdateTaskStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTaskStatus(int id, int taskStatusId, string? taskUpdate, string? returnUrl)
        {
            var note = await _context.Notes
                .Include(n => n.Contact)
                .FirstOrDefaultAsync(n => n.NoteId == id);

            if (note == null)
            {
                return NotFound();
            }

            if (!await CanAccessNoteAsync(note))
            {
                return Forbid();
            }

            var statusExists = await _context.TaskStatuses.AnyAsync(ts => ts.Id == taskStatusId);
            if (!statusExists)
            {
                return BadRequest("Invalid status.");
            }

            note.TaskStatusId = taskStatusId;
            if (!string.IsNullOrEmpty(taskUpdate))
            {
                note.TaskUpdate = taskUpdate.Trim();
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Task status updated.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Tasks));
        }

        // =====================
        // Private Helper Methods
        // =====================

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        private async Task<bool> IsManagerAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return false;
            return await _userManager.IsInRoleAsync(user, "Manager");
        }

        private async Task<bool> CanAccessContactAsync(Contact contact)
        {
            if (await IsManagerAsync())
            {
                return true;
            }

            return contact.SalesRepId == GetCurrentUserId();
        }

        private async Task<bool> CanAccessNoteAsync(Note note)
        {
            if (await IsManagerAsync())
            {
                return true;
            }

            if (note.Contact == null) return false;
            return note.Contact.SalesRepId == GetCurrentUserId();
        }

        private async Task<bool> CanDeleteNoteAsync(Note note)
        {
            if (await IsManagerAsync())
            {
                return true;
            }

            return note.SalesRepId == GetCurrentUserId()
                && note.Contact != null
                && note.Contact.SalesRepId == GetCurrentUserId();
        }

        private async Task<List<Note>> GetTasksAsync(bool pendingOnly)
        {
            var query = _context.Notes
                .AsNoTracking()
                .Include(n => n.Contact)
                .Include(n => n.SalesRep)
                .Include(n => n.TodoType)
                .Include(n => n.TodoDesc)
                .Include(n => n.TaskStatus)
                .Where(n => n.IsNewTodo);

            if (pendingOnly)
            {
                query = query.Where(n => n.TaskStatusId == 1); // Pending
            }
            else
            {
                query = query.Where(n => n.TaskStatusId == 2); // Completed
            }

            if (!await IsManagerAsync())
            {
                var userId = GetCurrentUserId();
                query = query.Where(n => n.Contact!.SalesRepId == userId);
            }

            return await query
                .OrderBy(n => n.TodoDueDate)
                .ToListAsync();
        }

        private async Task PopulateDropdownsAsync()
        {
            ViewBag.TodoTypes = new SelectList(
                await _context.TodoTypes.OrderBy(tt => tt.Id).ToListAsync(),
                "Id",
                "Type"
            );

            ViewBag.TodoDescs = new SelectList(
                await _context.TodoDescs.OrderBy(td => td.Id).ToListAsync(),
                "Id",
                "Description"
            );

            ViewBag.TaskStatuses = new SelectList(
                await _context.TaskStatuses.OrderBy(ts => ts.Id).ToListAsync(),
                "Id",
                "Status"
            );
        }
    }
}
