using FocusSpace.Application.DTOs;
using FocusSpace.Application.Services;
using FocusSpace.Domain.Entities;
using FocusSpace.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FocusSpace.Api.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public HomeController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var planets = await _context.Planets
                .AsNoTracking()
                .OrderBy(p => p.OrderNumber)
                .ToListAsync();

            int currentOrderNumber = 1;
            long totalMinutes = 0;

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user is not null)
                {
                    totalMinutes = user.TotalFocusMinutes;
                    var currentPlanet = planets.FirstOrDefault(p => p.Id == user.CurrentPlanetId);
                    currentOrderNumber = currentPlanet?.OrderNumber ?? 1;
                }
            }

            long? minutesToNext = null;
            var nextPlanet = planets.FirstOrDefault(p => p.OrderNumber == currentOrderNumber + 1);
            if (nextPlanet is not null
                && UserProgressService.ThresholdByOrder.TryGetValue(nextPlanet.OrderNumber, out var required))
            {
                minutesToNext = Math.Max(0, required - totalMinutes);
            }

            return View(new HomeViewModel
            {
                Planets = planets,
                CurrentPlanetOrderNumber = currentOrderNumber,
                TotalFocusMinutes = totalMinutes,
                MinutesToNextPlanet = minutesToNext,
                PlanetThresholds = UserProgressService.ThresholdByOrder
            });
        }
    }
}
