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

            var leadCount = await contactsQuery
                .CountAsync(c => c.ContactStatus!.Name == "Lead");

            var opportunityCount = await contactsQuery
                .CountAsync(c => c.ContactStatus!.Name == "Opportunity");

            var customerCount = await contactsQuery
                .CountAsync(c => c.ContactStatus!.Name == "Customer");

            var totalContacts = await contactsQuery.CountAsync();

            var recentContacts = await contactsQuery
                .Include(c => c.ContactStatus)
                .Include(c => c.AssignedTo)
                .OrderByDescending(c => c.UpdatedAt)
                .Take(5)
                .ToListAsync();

            IQueryable<Note> notesQuery = _context.Notes
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
