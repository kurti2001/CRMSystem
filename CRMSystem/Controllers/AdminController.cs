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

        // GET: Admin/MySalesReps - Main Manager view showing sales reps with their contacts
        public async Task<IActionResult> MySalesReps()
        {
            var salesReps = await _userManager.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            var repStats = new List<SalesRepWithContacts>();
            foreach (var rep in salesReps)
            {
                var roles = await _userManager.GetRolesAsync(rep);
                if (roles.Contains("SalesRep"))
                {
                    var contacts = await _context.Contacts
                        .AsNoTracking()
                        .Include(c => c.ContactStatus)
                        .Include(c => c.Notes.OrderByDescending(n => n.Date))
                            .ThenInclude(n => n.TodoType)
                        .Include(c => c.Notes)
                            .ThenInclude(n => n.TodoDesc)
                        .Include(c => c.Notes)
                            .ThenInclude(n => n.TaskStatus)
                        .Where(c => c.SalesRepId == rep.Id)
                        .OrderByDescending(c => c.UpdatedAt)
                        .ToListAsync();

                    repStats.Add(new SalesRepWithContacts
                    {
                        UserId = rep.Id,
                        FullName = rep.FullName,
                        Email = rep.Email!,
                        Contacts = contacts
                    });
                }
            }

            return View(repStats);
        }

        // GET: Admin/Tasks - All tasks view for managers
        public async Task<IActionResult> Tasks(string? salesRepId, bool showCompleted = false)
        {
            var query = _context.Notes
                .AsNoTracking()
                .Include(n => n.Contact)
                    .ThenInclude(c => c!.SalesRep)
                .Include(n => n.SalesRep)
                .Include(n => n.TodoType)
                .Include(n => n.TodoDesc)
                .Include(n => n.TaskStatus)
                .Where(n => n.IsNewTodo);

            if (!string.IsNullOrEmpty(salesRepId))
            {
                query = query.Where(n => n.Contact!.SalesRepId == salesRepId);
            }

            if (showCompleted)
            {
                query = query.Where(n => n.TaskStatusId == 2); // Completed
            }
            else
            {
                query = query.Where(n => n.TaskStatusId == 1); // Pending
            }

            var tasks = await query
                .OrderBy(n => n.TodoDueDate)
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

            ViewBag.SelectedSalesRepId = salesRepId;
            ViewBag.ShowCompleted = showCompleted;
            ViewBag.TotalCount = tasks.Count;

            return View(tasks);
        }

        // GET: Admin/Contacts - All contacts view for managers
        public async Task<IActionResult> Contacts(string? salesRepId, string? status)
        {
            var query = _context.Contacts
                .AsNoTracking()
                .Include(c => c.ContactStatus)
                .Include(c => c.SalesRep)
                .Include(c => c.Notes.OrderByDescending(n => n.Date))
                    .ThenInclude(n => n.TodoType)
                .Include(c => c.Notes)
                    .ThenInclude(n => n.TodoDesc)
                .Include(c => c.Notes)
                    .ThenInclude(n => n.TaskStatus)
                .AsQueryable();

            if (!string.IsNullOrEmpty(salesRepId))
            {
                query = query.Where(c => c.SalesRepId == salesRepId);
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
                await _context.ContactStatuses.OrderBy(cs => cs.ContactStatusId).ToListAsync(),
                "Name",
                "Name",
                status
            );

            ViewBag.SelectedSalesRepId = salesRepId;
            ViewBag.SelectedStatus = status;
            ViewBag.TotalCount = contacts.Count;

            return View(contacts);
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var contactCountsByUser = await _context.Contacts
                .GroupBy(c => c.SalesRepId)
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

    public class SalesRepWithContacts
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<Contact> Contacts { get; set; } = new List<Contact>();
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
