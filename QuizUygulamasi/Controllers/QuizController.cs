using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizUygulamasi.Data;
using QuizUygulamasi.Models;
using System.Linq;
using System.Threading.Tasks;
using static QuizUygulamasi.Models.ErrorViewModel;

namespace QuizUygulamasi.Controllers
{
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuizController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Quiz Listesi
        public async Task<IActionResult> Index()
        {
            var quizzes = await _context.Quizzes.ToListAsync();
            return View(quizzes);
        }

        // Yeni Quiz Oluşturma Sayfası
        public IActionResult Create()
        {
            return View();
        }

        // Yeni Quiz Oluşturma İşlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Quiz quiz)
        {   quiz.Questions = new List<Question>(); // Yeni quiz oluştururken soruları başlatıyoruz.
            if (ModelState.IsValid)
            {
                _context.Add(quiz);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(quiz);
        }

        // Quiz Düzenleme Sayfası
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }
            return View(quiz);
        }

        // Quiz Düzenleme İşlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id", "Name")] Quiz quiz)
        {
            if (id != quiz.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quiz);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuizExists(quiz.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(quiz);
        }

        // Quiz Silme Sayfası
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(m => m.Id == id);
            if (quiz == null)
            {
                return NotFound();
            }
            return View(quiz);
        }

        // Quiz Silme İşlemi
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz != null)
            {
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool QuizExists(int id)
        {
            return _context.Quizzes.Any(e => e.Id == id);
        }


        // Quiz Soruları Listesi
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(m => m.Id == id);
            if (quiz == null)
            {
                return NotFound();
            }
            return View(quiz);
        }

        // Yeni Soru Oluşturma Sayfası
        public IActionResult CreateQuestion(int quizId)
        {
            ViewBag.QuizId = quizId;
            return View();
        }

        // Yeni Soru Oluşturma İşlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuestion(int quizId, [Bind("Text", "OptionA", "OptionB", "OptionC", "OptionD", "CorrectAnswer")] Question question)
        {
            if (ModelState.IsValid)
            {
                question.QuizId = quizId;
                _context.Add(question);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = quizId });
            }
            ViewBag.QuizId = quizId;
            return View(question);
        }

        // Soru Düzenleme Sayfası
        public async Task<IActionResult> EditQuestion(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }
            ViewBag.QuizId = question.QuizId;
            return View(question);
        }

        // Soru Düzenleme İşlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(int id, [Bind("Id", "QuizId", "Text", "OptionA", "OptionB", "OptionC", "OptionD", "CorrectAnswer")] Question question)
        {
            if (id != question.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(question);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(question.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = question.QuizId });
            }
            ViewBag.QuizId = question.QuizId;
            return View(question);
        }

        // Soru Silme Sayfası
        public async Task<IActionResult> DeleteQuestion(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }
            ViewBag.QuizId = question.QuizId;
            return View(question);
        }

        // Soru Silme İşlemi
        [HttpPost, ActionName("DeleteQuestion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestionConfirmed(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                int quizId = question.QuizId;
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = quizId });
            }
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }





        public async Task<IActionResult> EnterLobby(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            // Aktif bir oda varsa onu göster, yoksa yeni bir oda kodu oluştur
            var existingRoom = await _context.QuizRooms.FirstOrDefaultAsync(r => r.QuizId == id && r.IsActive == false);
            if (existingRoom != null)
            {
                ViewBag.RoomCode = existingRoom.RoomCode;
                ViewBag.QuizId = id;
                return View();
            }
            else
            {
                // Yeni oda kodu oluşturmak için JavaScript'ten SignalR'ı kullanacağız.
                ViewBag.QuizId = id;
                return View(); // Sadece quiz id'sini gönderiyoruz.
            }
        }

        public IActionResult JoinQuiz()
        {
            return View();
        }

        public IActionResult Play(string id) // Oda Kodu Alınacak
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            ViewBag.RoomCode = id;
            return View();
        }
    }
}