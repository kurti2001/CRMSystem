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
            var contacts = await GetContactsByStatusAsync("lead");
            ViewData["Title"] = "My Leads";
            ViewData["StatusName"] = "Lead";
            ViewData["StatusId"] = 1;
            return View("Index", contacts);
        }

        // GET: Contacts/Opportunities (Proposal stage)
        public async Task<IActionResult> Opportunities()
        {
            var contacts = await GetContactsByStatusAsync("proposal");
            ViewData["Title"] = "My Opportunities";
            ViewData["StatusName"] = "Opportunity";
            ViewData["StatusId"] = 2;
            return View("Index", contacts);
        }

        // GET: Contacts/Customers
        public async Task<IActionResult> Customers()
        {
            var contacts = await GetContactsByStatusAsync("customer/won");
            ViewData["Title"] = "My Customers/Won";
            ViewData["StatusName"] = "Customer/Won";
            ViewData["StatusId"] = 3;
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
                .AsNoTracking()
                .Include(c => c.ContactStatus)
                .Include(c => c.SalesRep)
                .Include(c => c.Notes.OrderByDescending(n => n.Date))
                    .ThenInclude(n => n.SalesRep)
                .Include(c => c.Notes)
                    .ThenInclude(n => n.TodoType)
                .Include(c => c.Notes)
                    .ThenInclude(n => n.TodoDesc)
                .Include(c => c.Notes)
                    .ThenInclude(n => n.TaskStatus)
                .FirstOrDefaultAsync(c => c.ContactId == id);

            if (contact == null)
            {
                return NotFound();
            }

            if (!await CanAccessContactAsync(contact))
            {
                return Forbid();
            }

            // Load dropdowns for adding notes
            await PopulateNoteDropdownsAsync();

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
        public async Task<IActionResult> Create([Bind("ContactTitle,FirstName,MiddleName,LastName,Email,Phone,Company,Industry,Address,AddressStreet1,AddressStreet2,AddressCity,AddressState,AddressZip,AddressCountry,Title,Website,LinkedInProfile,BackgroundInfo,LeadReferralSource,DateOfInitialContact,ContactStatusId,SalesRepId,Rating,ProjectType,ProjectDescription,ProposalDueDate,Budget,Deliverables")] Contact contact)
        {
            if (!await IsValidAssignmentAsync(contact.SalesRepId))
            {
                ModelState.AddModelError("SalesRepId", "Invalid user assignment.");
            }

            if (ModelState.IsValid)
            {
                contact.FirstName = contact.FirstName?.Trim() ?? string.Empty;
                contact.LastName = contact.LastName?.Trim() ?? string.Empty;
                contact.Email = contact.Email?.Trim() ?? string.Empty;
                contact.Phone = contact.Phone?.Trim();
                contact.Company = contact.Company?.Trim();

                contact.CreatedAt = DateTime.UtcNow;
                contact.UpdatedAt = DateTime.UtcNow;

                if (contact.DateOfInitialContact == null)
                {
                    contact.DateOfInitialContact = DateTime.UtcNow;
                }

                if (!User.IsInRole("Manager"))
                {
                    contact.SalesRepId = GetCurrentUserId();
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
        public async Task<IActionResult> Edit(int id, [Bind("ContactId,ContactTitle,FirstName,MiddleName,LastName,Email,Phone,Company,Industry,Address,AddressStreet1,AddressStreet2,AddressCity,AddressState,AddressZip,AddressCountry,Title,Website,LinkedInProfile,BackgroundInfo,LeadReferralSource,DateOfInitialContact,ContactStatusId,SalesRepId,Rating,ProjectType,ProjectDescription,ProposalDueDate,Budget,Deliverables")] Contact contact)
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

            if (!await IsValidAssignmentAsync(contact.SalesRepId))
            {
                ModelState.AddModelError("SalesRepId", "Invalid user assignment.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    contact.CreatedAt = existingContact.CreatedAt;
                    contact.UpdatedAt = DateTime.UtcNow;

                    if (!User.IsInRole("Manager"))
                    {
                        contact.SalesRepId = existingContact.SalesRepId;
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

            return contact.SalesRepId == GetCurrentUserId();
        }

        private async Task<List<Contact>> GetContactsByStatusAsync(string statusName)
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
                .Where(c => c.ContactStatus!.Name == statusName);

            if (!await IsManagerAsync())
            {
                var userId = GetCurrentUserId();
                query = query.Where(c => c.SalesRepId == userId);
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

            return await _context.Users.AnyAsync(u => u.Id == userId && u.IsActive);
        }

        private async Task<bool> ContactExistsAsync(int id)
        {
            return await _context.Contacts.AnyAsync(c => c.ContactId == id);
        }

        private async Task PopulateDropdownsAsync()
        {
            ViewBag.ContactStatuses = new SelectList(
                await _context.ContactStatuses.OrderBy(cs => cs.ContactStatusId).ToListAsync(),
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

        private async Task PopulateNoteDropdownsAsync()
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
