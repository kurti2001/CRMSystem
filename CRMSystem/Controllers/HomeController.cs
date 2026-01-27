// File: Controllers/HomeController.cs

using System.Diagnostics;
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
    public class HomeController : Controller
    {
        private readonly CrmDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(CrmDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Home/Index (Dashboard)
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var isManager = await IsManagerAsync();

            IQueryable<Contact> contactsQuery = _context.Contacts;

            if (!isManager)
            {
                contactsQuery = contactsQuery.Where(c => c.AssignedToId == userId);
            }

            // Single query: counts grouped by status name
            var contactCounts = await contactsQuery
                .GroupBy(c => c.ContactStatus!.Name)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalContacts = contactCounts.Sum(x => x.Count);
            var leadCount = contactCounts.FirstOrDefault(x => x.Status == "Lead")?.Count ?? 0;
            var opportunityCount = contactCounts.FirstOrDefault(x => x.Status == "Opportunity")?.Count ?? 0;
            var customerCount = contactCounts.FirstOrDefault(x => x.Status == "Customer")?.Count ?? 0;

            var recentContacts = await contactsQuery
                .AsNoTracking()
                .Include(c => c.ContactStatus)
                .Include(c => c.AssignedTo)
                .OrderByDescending(c => c.UpdatedAt)
                .Take(5)
                .ToListAsync();

            IQueryable<Note> notesQuery = _context.Notes
                .AsNoTracking()
                .Include(n => n.Contact)
                .Include(n => n.Author);

            if (!isManager)
            {
                notesQuery = notesQuery.Where(n => n.Contact!.AssignedToId == userId);
            }

            var recentNotes = await notesQuery
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.LeadCount = leadCount;
            ViewBag.OpportunityCount = opportunityCount;
            ViewBag.CustomerCount = customerCount;
            ViewBag.TotalContacts = totalContacts;
            ViewBag.RecentContacts = recentContacts;
            ViewBag.RecentNotes = recentNotes;
            ViewBag.IsManager = isManager;

            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
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
    }
}
