using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortofolioKazhuro.Context;
using PortofolioKazhuro.Models;
using PortofolioKazhuro.ViewModel;
using System.Diagnostics;

namespace PortofolioKazhuro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PortfolioContext _context;

        public HomeController(ILogger<HomeController> logger, PortfolioContext context)
        {
            _context = context;


            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
              var model = new AdminViewModel
            {
                Profile = await _context.Profiles.FirstAsync(),
                educations = await _context.educations.ToListAsync(),
                Projects = await _context.Projects.ToListAsync(),
                Skills = await _context.Skills.ToListAsync(),
                Certificates = await _context.Certificates.ToListAsync(),
                experiences = await _context.Experiences.ToListAsync(),
                visitorStats = await _context.VisitorStats.ToListAsync(),


            };
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
