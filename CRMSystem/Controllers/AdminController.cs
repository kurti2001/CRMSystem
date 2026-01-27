// File: Controllers/AdminController.cs

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
    [Authorize(Roles = "Manager")]
    public class AdminController : Controller
    {
        private readonly CrmDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(CrmDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Single query: contact counts grouped by status
            var contactCounts = await _context.Contacts
                .GroupBy(c => c.ContactStatus!.Name)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalContacts = contactCounts.Sum(x => x.Count);
            var leadCount = contactCounts.FirstOrDefault(x => x.Status == "Lead")?.Count ?? 0;
            var opportunityCount = contactCounts.FirstOrDefault(x => x.Status == "Opportunity")?.Count ?? 0;
            var customerCount = contactCounts.FirstOrDefault(x => x.Status == "Customer")?.Count ?? 0;

            // Single query: note counts grouped by type and overdue status
            var now = DateTime.UtcNow;
            var noteCounts = await _context.Notes
                .Where(n => !n.IsCompleted && (n.Type == NoteType.Task || n.Type == NoteType.Meeting))
                .GroupBy(n => new { n.Type, IsOverdue = n.DueDate < now })
                .Select(g => new { g.Key.Type, g.Key.IsOverdue, Count = g.Count() })
                .ToListAsync();

            var openTasks = noteCounts.Where(x => x.Type == NoteType.Task).Sum(x => x.Count);
            var openMeetings = noteCounts.Where(x => x.Type == NoteType.Meeting).Sum(x => x.Count);
            var overdueTasks = noteCounts.Where(x => x.Type == NoteType.Task && x.IsOverdue).Sum(x => x.Count);
            var overdueMeetings = noteCounts.Where(x => x.Type == NoteType.Meeting && x.IsOverdue).Sum(x => x.Count);

            // Single query: user counts
            var userCounts = await _userManager.Users
                .GroupBy(u => u.IsActive)
                .Select(g => new { IsActive = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalUsers = userCounts.Sum(x => x.Count);
            var activeUsers = userCounts.FirstOrDefault(x => x.IsActive)?.Count ?? 0;

            // Batch query: contact counts per user
            var contactCountsByUser = await _context.Contacts
                .GroupBy(c => c.AssignedToId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            // Batch query: open task/meeting counts per user
            var noteCountsByUser = await _context.Notes
                .Where(n => !n.IsCompleted && (n.Type == NoteType.Task || n.Type == NoteType.Meeting))
                .GroupBy(n => new { UserId = n.Contact!.AssignedToId, n.Type })
                .Select(g => new { g.Key.UserId, g.Key.Type, Count = g.Count() })
                .ToListAsync();

            // Get active users with their roles via join on UserRoles
            var salesReps = await _userManager.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            var repStats = new List<SalesRepStat>();
            foreach (var rep in salesReps)
            {
                var roles = await _userManager.GetRolesAsync(rep);
                repStats.Add(new SalesRepStat
                {
                    UserId = rep.Id,
                    FullName = rep.FullName,
                    Email = rep.Email!,
                    Role = roles.FirstOrDefault() ?? "None",
                    ContactCount = contactCountsByUser.GetValueOrDefault(rep.Id, 0),
                    OpenTaskCount = noteCountsByUser
                        .Where(x => x.UserId == rep.Id && x.Type == NoteType.Task)
                        .Sum(x => x.Count),
                    OpenMeetingCount = noteCountsByUser
                        .Where(x => x.UserId == rep.Id && x.Type == NoteType.Meeting)
                        .Sum(x => x.Count)
                });
            }

            ViewBag.TotalContacts = totalContacts;
            ViewBag.LeadCount = leadCount;
            ViewBag.OpportunityCount = opportunityCount;
            ViewBag.CustomerCount = customerCount;
            ViewBag.OpenTasks = openTasks;
            ViewBag.OpenMeetings = openMeetings;
            ViewBag.OverdueTasks = overdueTasks;
            ViewBag.OverdueMeetings = overdueMeetings;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.ActiveUsers = activeUsers;
            ViewBag.RepStats = repStats;

            return View();
        }

        // GET: Admin/Contacts?salesRepId=xxx
        public async Task<IActionResult> Contacts(string? salesRepId, string? status)
        {
            var query = _context.Contacts
                .AsNoTracking()
                .Include(c => c.ContactStatus)
                .Include(c => c.AssignedTo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(salesRepId))
            {
                query = query.Where(c => c.AssignedToId == salesRepId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(c => c.ContactStatus!.Name == status);
            }

            var contacts = await query
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();

            var users = await _userManager.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            ViewBag.Users = new SelectList(
                users.Select(u => new { u.Id, Name = u.FullName }),
                "Id",
                "Name",
                salesRepId
            );

            ViewBag.Statuses = new SelectList(
                await _context.ContactStatuses.OrderBy(cs => cs.Name).ToListAsync(),
                "Name",
                "Name",
                status
            );

            ViewBag.SelectedSalesRepId = salesRepId;
            ViewBag.SelectedStatus = status;
            ViewBag.TotalCount = contacts.Count;

            return View(contacts);
        }

        // GET: Admin/AllTasks?salesRepId=xxx
        public async Task<IActionResult> AllTasks(string? salesRepId, bool showCompleted = false)
        {
            var query = _context.Notes
                .AsNoTracking()
                .Include(n => n.Contact)
                .Include(n => n.Author)
                .Where(n => n.Type == NoteType.Task && n.IsCompleted == showCompleted);

            if (!string.IsNullOrEmpty(salesRepId))
            {
                query = query.Where(n => n.Contact!.AssignedToId == salesRepId);
            }

            if (showCompleted)
            {
                query = query.OrderByDescending(n => n.CompletedAt);
            }
            else
            {
                query = query.OrderBy(n => n.DueDate);
            }

            var tasks = await query.ToListAsync();

            var users = await _userManager.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            ViewBag.Users = new SelectList(
                users.Select(u => new { u.Id, Name = u.FullName }),
                "Id",
                "Name",
                salesRepId
            );

            ViewBag.SelectedSalesRepId = salesRepId;
            ViewBag.ShowCompleted = showCompleted;
            ViewBag.TotalCount = tasks.Count;

            return View(tasks);
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            // Batch query: contact counts per user (avoids N+1)
            var contactCountsByUser = await _context.Contacts
                .GroupBy(c => c.AssignedToId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var users = await _userManager.Users
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            var userList = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email!,
                    Role = roles.FirstOrDefault() ?? "None",
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    ContactCount = contactCountsByUser.GetValueOrDefault(user.Id, 0)
                });
            }

            return View(userList);
        }

        // POST: Admin/ToggleUserStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot deactivate your own account.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            var statusText = user.IsActive ? "activated" : "deactivated";
            TempData["SuccessMessage"] = $"User {user.FullName} has been {statusText}.";

            return RedirectToAction(nameof(Users));
        }

        // POST: Admin/ChangeUserRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserRole(string userId, string newRole)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newRole))
            {
                return BadRequest();
            }

            // Validate role against allowed roles only
            var allowedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Manager", "SalesRep" };
            if (!allowedRoles.Contains(newRole))
            {
                TempData["ErrorMessage"] = "Invalid role specified.";
                return RedirectToAction(nameof(Users));
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot change your own role.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            TempData["SuccessMessage"] = $"User {user.FullName} role changed to {newRole}.";

            return RedirectToAction(nameof(Users));
        }
    }

    // =====================
    // View Models
    // =====================

    public class SalesRepStat
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int ContactCount { get; set; }
        public int OpenTaskCount { get; set; }
        public int OpenMeetingCount { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ContactCount { get; set; }
    }
}
