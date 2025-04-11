using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizUygulamasi.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QuizUygulamasi.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var participantsWithQuizInfo = await _context.Participants
                .Include(p => p.QuizRoom)
                .ThenInclude(qr => qr.Quiz)
                .OrderByDescending(p => p.Score)
                .ToListAsync();

            return View(participantsWithQuizInfo);
        }
    }
}