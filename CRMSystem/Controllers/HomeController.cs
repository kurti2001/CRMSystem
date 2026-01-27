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

        // GET: Home/Index - Redirect to appropriate default page based on role
        public async Task<IActionResult> Index()
        {
            var isManager = await IsManagerAsync();

            if (isManager)
            {
                // Managers go to My Sales Reps view
                return RedirectToAction("MySalesReps", "Admin");
            }
            else
            {
                // Sales Reps go to Tasks view
                return RedirectToAction("Tasks", "Notes");
            }
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
