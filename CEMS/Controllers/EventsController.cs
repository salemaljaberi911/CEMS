using CEMS.Data;
using CEMS.Models;
using CEMS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CEMS.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private const string PendingStatus = "Pending";
        private const string ApprovedStatus = "Approved";
        private const string RejectedStatus = "Rejected";

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================
        // Organizer Area
        // =========================

        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Index()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Challenge();
            }

            var userEvents = await _context.Events
                .AsNoTracking()
                .Where(e => e.OrganizerId == currentUser.Id)
                .OrderByDescending(e => e.EventDate)
                .Select(e => new OrganizerEventListViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    Capacity = e.Capacity,
                    Status = e.Status,
                    RegistrationCount = _context.Registrations.Count(r => r.EventId == e.Id),
                    ImageData = e.ImageData,
                    ImageContentType = e.ImageContentType
                })
                .ToListAsync();

            return View(userEvents);
        }

        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Details(int? id)
        {
            var selectedEvent = await GetAuthorizedEventAsync(id);

            if (selectedEvent == null)
            {
                return NotFound();
            }

            return View(selectedEvent);
        }

        [Authorize(Roles = "Organizer")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Organizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,Description,EventDate,Location,Capacity")] Event newEvent,
            IFormFile? imageFile)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                return View(newEvent);
            }

            newEvent.OrganizerId = currentUser.Id;
            newEvent.Status = PendingStatus;
            newEvent.EventDate = DateTime.SpecifyKind(newEvent.EventDate, DateTimeKind.Local).ToUniversalTime();

            if (imageFile != null && imageFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await imageFile.CopyToAsync(memoryStream);

                newEvent.ImageData = memoryStream.ToArray();
                newEvent.ImageContentType = imageFile.ContentType;
                newEvent.ImageFileName = imageFile.FileName;
            }
            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Event created successfully and submitted for approval.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Edit(int? id)
        {
            var selectedEvent = await GetAuthorizedEventAsync(id);

            if (selectedEvent == null)
            {
                return NotFound();
            }

            return View(selectedEvent);
        }

        [Authorize(Roles = "Organizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Title,Description,EventDate,Location,Capacity")] Event editedEvent,
            IFormFile? imageFile)
        {
            if (id != editedEvent.Id)
            {
                return NotFound();
            }

            var existingEvent = await GetAuthorizedEventAsync(id);
            if (existingEvent == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(existingEvent);
            }

            try
            {
                existingEvent.Title = editedEvent.Title;
                existingEvent.Description = editedEvent.Description;
                existingEvent.EventDate = DateTime.SpecifyKind(editedEvent.EventDate, DateTimeKind.Local).ToUniversalTime();
                existingEvent.Location = editedEvent.Location;
                existingEvent.Capacity = editedEvent.Capacity;

                if (imageFile != null && imageFile.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await imageFile.CopyToAsync(memoryStream);

                    existingEvent.ImageData = memoryStream.ToArray();
                    existingEvent.ImageContentType = imageFile.ContentType;
                    existingEvent.ImageFileName = imageFile.FileName;
                }

                await _context.SaveChangesAsync();

                TempData["Message"] = "Event updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty, "Unable to save changes. Please try again.");
                return View(existingEvent);
            }
        }

        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Delete(int? id)
        {
            var selectedEvent = await GetAuthorizedEventAsync(id);

            if (selectedEvent == null)
            {
                return NotFound();
            }

            return View(selectedEvent);
        }

        [Authorize(Roles = "Organizer")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var selectedEvent = await GetAuthorizedEventAsync(id);

            if (selectedEvent == null)
            {
                return NotFound();
            }

            _context.Events.Remove(selectedEvent);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Event deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> MyDashboard()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Challenge();
            }

            var now = DateTime.UtcNow;

            var myEventsQuery = _context.Events
                .Where(e => e.OrganizerId == currentUser.Id);

            var myEventIds = await myEventsQuery
                .Select(e => e.Id)
                .ToListAsync();

            var eventRegistrationStats = await _context.Events
                .Where(e => e.OrganizerId == currentUser.Id)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.EventDate,
                    RegistrationCount = _context.Registrations.Count(r => r.EventId == e.Id)
                })
                .ToListAsync();

            var mostRegisteredEvent = eventRegistrationStats
                .OrderByDescending(e => e.RegistrationCount)
                .ThenBy(e => e.Title)
                .FirstOrDefault();

            var nextUpcomingEvent = eventRegistrationStats
                .Where(e => e.EventDate >= now)
                .OrderBy(e => e.EventDate)
                .FirstOrDefault();

            var dashboard = new OrganizerDashboardViewModel
            {
                TotalEvents = await myEventsQuery.CountAsync(),
                PendingEvents = await myEventsQuery.CountAsync(e => e.Status == PendingStatus),
                ApprovedEvents = await myEventsQuery.CountAsync(e => e.Status == ApprovedStatus),
                RejectedEvents = await myEventsQuery.CountAsync(e => e.Status == RejectedStatus),
                UpcomingEvents = await myEventsQuery.CountAsync(e => e.EventDate >= now),
                PastEvents = await myEventsQuery.CountAsync(e => e.EventDate < now),
                TotalRegistrations = await _context.Registrations.CountAsync(r => myEventIds.Contains(r.EventId)),
                MostRegisteredEventTitle = mostRegisteredEvent?.Title,
                MostRegisteredEventCount = mostRegisteredEvent?.RegistrationCount ?? 0,
                NextUpcomingEventTitle = nextUpcomingEvent?.Title,
                NextUpcomingEventDate = nextUpcomingEvent?.EventDate
            };

            return View(dashboard);
        }

        // =========================
        // Public / Student Area
        // =========================

        [AllowAnonymous]
        public async Task<IActionResult> ApprovedEvents()
        {
            var approvedEvents = await _context.Events
                .AsNoTracking()
                .Where(e => e.Status == ApprovedStatus)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return View(approvedEvents);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ApprovedDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var approvedEvent = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id && e.Status == ApprovedStatus);

            if (approvedEvent == null)
            {
                return NotFound();
            }

            var currentRegistrationCount = await _context.Registrations
                .CountAsync(r => r.EventId == approvedEvent.Id);

            bool isAlreadyRegistered = false;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser != null)
                {
                    isAlreadyRegistered = await _context.Registrations
                        .AnyAsync(r => r.EventId == approvedEvent.Id && r.UserId == currentUser.Id);
                }
            }

            ViewBag.CurrentRegistrationCount = currentRegistrationCount;
            ViewBag.RemainingSeats = approvedEvent.Capacity - currentRegistrationCount;
            ViewBag.IsAlreadyRegistered = isAlreadyRegistered;

            return View(approvedEvent);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterForEvent(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Challenge();
            }

            var approvedEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id && e.Status == ApprovedStatus);

            if (approvedEvent == null)
            {
                return NotFound();
            }

            var currentRegistrationCount = await _context.Registrations
                .CountAsync(r => r.EventId == id);

            if (currentRegistrationCount >= approvedEvent.Capacity)
            {
                TempData["Message"] = "Sorry, this event is already full.";
                return RedirectToAction(nameof(ApprovedDetails), new { id });
            }

            var existingRegistration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.EventId == id && r.UserId == currentUser.Id);

            if (existingRegistration != null)
            {
                TempData["Message"] = "You have already registered for this event.";
                return RedirectToAction(nameof(ApprovedDetails), new { id });
            }

            var registration = new Registration
            {
                EventId = id,
                UserId = currentUser.Id,
                RegisteredAt = DateTime.UtcNow
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Registration completed successfully.";
            return RedirectToAction(nameof(ApprovedDetails), new { id });
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyRegistrations()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Challenge();
            }

            var myRegistrations = await _context.Registrations
                .Where(r => r.UserId == currentUser.Id)
                .Join(
                    _context.Events,
                    registration => registration.EventId,
                    ev => ev.Id,
                    (registration, ev) => new MyRegistrationViewModel
                    {
                        RegistrationId = registration.Id,
                        EventId = ev.Id,
                        Title = ev.Title,
                        EventDate = ev.EventDate,
                        Location = ev.Location,
                        RegisteredAt = registration.RegisteredAt
                    })
                .OrderByDescending(r => r.EventDate)
                .ToListAsync();

            return View(myRegistrations);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRegistration(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Challenge();
            }

            var registration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

            if (registration == null)
            {
                return NotFound();
            }

            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Your registration has been cancelled successfully.";
            return RedirectToAction(nameof(MyRegistrations));
        }

        // =========================
        // Staff Coordinator Area
        // =========================

        [Authorize(Roles = "StaffCoordinator")]
        public async Task<IActionResult> PendingApproval()
        {
            var pendingEvents = await _context.Events
                .AsNoTracking()
                .Where(e => e.Status == PendingStatus)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return View(pendingEvents);
        }

        [HttpPost]
        [Authorize(Roles = "StaffCoordinator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var eventToApprove = await _context.Events.FindAsync(id);

            if (eventToApprove == null)
            {
                return NotFound();
            }

            eventToApprove.Status = ApprovedStatus;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Event approved successfully.";
            return RedirectToAction(nameof(PendingApproval));
        }

        [HttpPost]
        [Authorize(Roles = "StaffCoordinator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var eventToReject = await _context.Events.FindAsync(id);

            if (eventToReject == null)
            {
                return NotFound();
            }

            eventToReject.Status = RejectedStatus;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Event rejected successfully.";
            return RedirectToAction(nameof(PendingApproval));
        }

        [Authorize(Roles = "StaffCoordinator")]
        public async Task<IActionResult> ManageAttendance(int id)
        {
            var selectedEvent = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (selectedEvent == null)
            {
                return NotFound();
            }

            var registrations = await _context.Registrations
                .Where(r => r.EventId == id)
                .Join(
                    _context.Users,
                    registration => registration.UserId,
                    user => user.Id,
                    (registration, user) => new AttendanceViewModel
                    {
                        RegistrationId = registration.Id,
                        EventId = registration.EventId,
                        UserId = registration.UserId,
                        Email = user.Email ?? "No Email",
                        FullName = user.FullName,
                        RegisteredAt = registration.RegisteredAt,
                        IsAttended = registration.IsAttended
                    })
                .OrderBy(r => r.FullName)
                .ToListAsync();

            ViewBag.EventId = id;
            ViewBag.EventTitle = selectedEvent.Title;

            return View(registrations);
        }

        [HttpPost]
        [Authorize(Roles = "StaffCoordinator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(int id)
        {
            var registration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration == null)
            {
                return NotFound();
            }

            if (!registration.IsAttended)
            {
                registration.IsAttended = true;
                await _context.SaveChangesAsync();
            }

            TempData["Message"] = "Attendance has been marked successfully.";
            return RedirectToAction(nameof(ManageAttendance), new { id = registration.EventId });
        }

        // =========================
        // Private Helpers
        // =========================

        private async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(User);
        }

        private async Task<Event?> GetAuthorizedEventAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return null;
            }

            return await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id && e.OrganizerId == currentUser.Id);
        }
    }
}