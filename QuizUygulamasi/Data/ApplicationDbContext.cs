using Microsoft.EntityFrameworkCore;
using QuizUygulamasi.Models;
using static QuizUygulamasi.Models.ErrorViewModel;

namespace QuizUygulamasi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<QuizRoom> QuizRooms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Quiz ile Question arasındaki ilişki (varsayılan olarak Cascade)
            modelBuilder.Entity<Quiz>()
                .HasMany(q => q.Questions)
                .WithOne(qu => qu.Quiz)
                .HasForeignKey(qu => qu.QuizId)
                .OnDelete(DeleteBehavior.Cascade); // Bir quiz silindiğinde soruları da silinir.

            // QuizRoom ile Quiz arasındaki ilişki
            modelBuilder.Entity<QuizRoom>()
                .HasOne(qr => qr.Quiz)
                .WithMany() // Quiz tarafında bir navigation property'ye ihtiyacımız yoksa bu şekilde bırakılabilir.
                .HasForeignKey(qr => qr.QuizId)
                .OnDelete(DeleteBehavior.Restrict); // Bir quizRoom aktifken quiz silinemez.

            // QuizRoom ile Participant arasındaki ilişki (Cascade ayarlanabilir)
            modelBuilder.Entity<QuizRoom>()
                .HasMany(qr => qr.Participants)
                .WithOne(p => p.QuizRoom)
                .HasForeignKey(p => p.QuizRoomId)
                .OnDelete(DeleteBehavior.Cascade); // Bir quizRoom silindiğinde katılımcıları da silinir.
        }
    }
}