// File: Controllers/ContactsController.cs

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
    public class ContactsController : Controller
    {
        private readonly CrmDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContactsController(CrmDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Contacts/Leads
        public async Task<IActionResult> Leads()
        {
            var contacts = await GetContactsByStatusAsync("Lead");
            ViewData["Title"] = "Leads";
            ViewData["StatusName"] = "Lead";
            return View("Index", contacts);
        }

        // GET: Contacts/Opportunities
        public async Task<IActionResult> Opportunities()
        {
            var contacts = await GetContactsByStatusAsync("Opportunity");
            ViewData["Title"] = "Opportunities";
            ViewData["StatusName"] = "Opportunity";
            return View("Index", contacts);
        }

        // GET: Contacts/Customers
        public async Task<IActionResult> Customers()
        {
            var contacts = await GetContactsByStatusAsync("Customer");
            ViewData["Title"] = "Customers";
            ViewData["StatusName"] = "Customer";
            return View("Index", contacts);
        }

        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.ContactStatus)
                .Include(c => c.AssignedTo)
                .Include(c => c.Notes.OrderByDescending(n => n.CreatedAt))
                    .ThenInclude(n => n.Author)
                .FirstOrDefaultAsync(c => c.ContactId == id);

            if (contact == null)
            {
                return NotFound();
            }

            if (!await CanAccessContactAsync(contact))
            {
                return Forbid();
            }

            return View(contact);
        }

        // GET: Contacts/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View();
        }

        // POST: Contacts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FirstName,LastName,Email,Phone,Company,Address,ContactStatusId,AssignedToId")] Contact contact)
        {
            if (!await IsValidAssignmentAsync(contact.AssignedToId))
            {
                ModelState.AddModelError("AssignedToId", "Invalid user assignment.");
            }

            if (ModelState.IsValid)
            {
                contact.CreatedAt = DateTime.UtcNow;
                contact.UpdatedAt = DateTime.UtcNow;

                if (!User.IsInRole("Manager"))
                {
                    contact.AssignedToId = GetCurrentUserId();
                }

                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Contact created successfully.";
                return RedirectToStatusAction(contact.ContactStatusId);
            }

            await PopulateDropdownsAsync();
            return View(contact);
        }

        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.ContactStatus)
                .FirstOrDefaultAsync(c => c.ContactId == id);

            if (contact == null)
            {
                return NotFound();
            }

            if (!await CanAccessContactAsync(contact))
            {
                return Forbid();
            }

            await PopulateDropdownsAsync();
            return View(contact);
        }

        // POST: Contacts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("ContactId,FirstName,LastName,Email,Phone,Company,Address,ContactStatusId,AssignedToId")] Contact contact)
        {
            if (id != contact.ContactId)
            {
                return NotFound();
            }

            var existingContact = await _context.Contacts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ContactId == id);

            if (existingContact == null)
            {
                return NotFound();
            }

            if (!await CanAccessContactAsync(existingContact))
            {
                return Forbid();
            }

            if (!await IsValidAssignmentAsync(contact.AssignedToId))
            {
                ModelState.AddModelError("AssignedToId", "Invalid user assignment.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    contact.CreatedAt = existingContact.CreatedAt;
                    contact.UpdatedAt = DateTime.UtcNow;

                    if (!User.IsInRole("Manager"))
                    {
                        contact.AssignedToId = existingContact.AssignedToId;
                    }

                    _context.Update(contact);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Contact updated successfully.";
                    return RedirectToStatusAction(contact.ContactStatusId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ContactExistsAsync(contact.ContactId))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            await PopulateDropdownsAsync();
            return View(contact);
        }

        // POST: Contacts/ChangeStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int contactId, int newStatusId)
        {
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

            var statusExists = await _context.ContactStatuses
                .AnyAsync(cs => cs.ContactStatusId == newStatusId);

            if (!statusExists)
            {
                return BadRequest("Invalid status.");
            }

            int previousStatusId = contact.ContactStatusId;
            contact.ContactStatusId = newStatusId;
            contact.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Contact status changed successfully.";
            return RedirectToStatusAction(previousStatusId);
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

        private async Task<List<Contact>> GetContactsByStatusAsync(string statusName)
        {
            var query = _context.Contacts
                .Include(c => c.ContactStatus)
                .Include(c => c.AssignedTo)
                .Where(c => c.ContactStatus!.Name == statusName);

            if (!await IsManagerAsync())
            {
                var userId = GetCurrentUserId();
                query = query.Where(c => c.AssignedToId == userId);
            }

            return await query
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }

        private async Task<bool> IsValidAssignmentAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        private async Task<bool> ContactExistsAsync(int id)
        {
            return await _context.Contacts.AnyAsync(c => c.ContactId == id);
        }

        private async Task PopulateDropdownsAsync()
        {
            ViewBag.ContactStatuses = new SelectList(
                await _context.ContactStatuses.OrderBy(cs => cs.Name).ToListAsync(),
                "ContactStatusId",
                "Name"
            );

            if (await IsManagerAsync())
            {
                var users = await _userManager.Users
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .ToListAsync();

                ViewBag.Users = new SelectList(
                    users.Select(u => new { u.Id, Name = u.FullName }),
                    "Id",
                    "Name"
                );
            }
            else
            {
                var currentUserId = GetCurrentUserId();
                var currentUser = await _userManager.FindByIdAsync(currentUserId);

                ViewBag.Users = new SelectList(
                    new[] { new { Id = currentUser!.Id, Name = currentUser.FullName } },
                    "Id",
                    "Name"
                );
            }
        }

        private IActionResult RedirectToStatusAction(int statusId)
        {
            return statusId switch
            {
                1 => RedirectToAction(nameof(Leads)),
                2 => RedirectToAction(nameof(Opportunities)),
                3 => RedirectToAction(nameof(Customers)),
                _ => RedirectToAction(nameof(Leads))
            };
        }
    }
}
