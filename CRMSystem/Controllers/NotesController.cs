// File: Controllers/NotesController.cs

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        // GET: Notes/Tasks
        public async Task<IActionResult> Tasks()
        {
            var notes = await GetNotesByTypeAsync(NoteType.Task, completedFilter: false);
            ViewData["Title"] = "Open Tasks";
            ViewData["NoteType"] = "Task";
            return View("List", notes);
        }

        // GET: Notes/Meetings
        public async Task<IActionResult> Meetings()
        {
            var notes = await GetNotesByTypeAsync(NoteType.Meeting, completedFilter: false);
            ViewData["Title"] = "Upcoming Meetings";
            ViewData["NoteType"] = "Meeting";
            return View("List", notes);
        }

        // GET: Notes/CompletedTasks
        public async Task<IActionResult> CompletedTasks()
        {
            var notes = await GetNotesByTypeAsync(NoteType.Task, completedFilter: true);
            ViewData["Title"] = "Completed Tasks";
            ViewData["NoteType"] = "Task";
            ViewData["ShowCompleted"] = true;
            return View("List", notes);
        }

        // GET: Notes/CompletedMeetings
        public async Task<IActionResult> CompletedMeetings()
        {
            var notes = await GetNotesByTypeAsync(NoteType.Meeting, completedFilter: true);
            ViewData["Title"] = "Completed Meetings";
            ViewData["NoteType"] = "Meeting";
            ViewData["ShowCompleted"] = true;
            return View("List", notes);
        }

        // POST: Notes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int contactId, string content, NoteType type, DateTime? dueDate)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Content cannot be empty.";
                return RedirectToAction("Details", "Contacts", new { id = contactId });
            }

            if (content.Length > 4000)
            {
                TempData["ErrorMessage"] = "Content cannot exceed 4000 characters.";
                return RedirectToAction("Details", "Contacts", new { id = contactId });
            }

            if ((type == NoteType.Task || type == NoteType.Meeting) && dueDate == null)
            {
                TempData["ErrorMessage"] = $"Due date is required for a {type}.";
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
                Type = type,
                DueDate = dueDate,
                IsCompleted = false,
                ContactId = contactId,
                AuthorId = GetCurrentUserId(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            var label = type == NoteType.Note ? "Note" : type == NoteType.Task ? "Task" : "Meeting";
            TempData["SuccessMessage"] = $"{label} added successfully.";
            return RedirectToAction("Details", "Contacts", new { id = contactId });
        }

        // POST: Notes/MarkCompleted/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkCompleted(int id, string? returnUrl)
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

            if (note.Type == NoteType.Note)
            {
                TempData["ErrorMessage"] = "Notes cannot be marked as completed.";
                return RedirectToAction("Details", "Contacts", new { id = note.ContactId });
            }

            note.IsCompleted = true;
            note.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var label = note.Type == NoteType.Task ? "Task" : "Meeting";
            TempData["SuccessMessage"] = $"{label} marked as completed.";

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

            if (note.Type == NoteType.Note)
            {
                TempData["ErrorMessage"] = "Notes cannot be reopened.";
                return RedirectToAction("Details", "Contacts", new { id = note.ContactId });
            }

            note.IsCompleted = false;
            note.CompletedAt = null;

            await _context.SaveChangesAsync();

            var label = note.Type == NoteType.Task ? "Task" : "Meeting";
            TempData["SuccessMessage"] = $"{label} reopened.";

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

            return contact.AssignedToId == GetCurrentUserId();
        }

        private async Task<bool> CanAccessNoteAsync(Note note)
        {
            if (await IsManagerAsync())
            {
                return true;
            }

            if (note.Contact == null) return false;
            return note.Contact.AssignedToId == GetCurrentUserId();
        }

        private async Task<bool> CanDeleteNoteAsync(Note note)
        {
            if (await IsManagerAsync())
            {
                return true;
            }

            return note.AuthorId == GetCurrentUserId()
                && note.Contact != null
                && note.Contact.AssignedToId == GetCurrentUserId();
        }

        private async Task<List<Note>> GetNotesByTypeAsync(NoteType type, bool completedFilter)
        {
            var query = _context.Notes
                .Include(n => n.Contact)
                .Include(n => n.Author)
                .Where(n => n.Type == type && n.IsCompleted == completedFilter);

            if (!await IsManagerAsync())
            {
                var userId = GetCurrentUserId();
                query = query.Where(n => n.Contact!.AssignedToId == userId);
            }

            if (completedFilter)
            {
                query = query.OrderByDescending(n => n.CompletedAt);
            }
            else
            {
                query = query.OrderBy(n => n.DueDate);
            }

            return await query.ToListAsync();
        }
    }
}
