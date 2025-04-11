using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizUygulamasi.Models
{
    public class Participant
    {
        public int Id { get; set; }
        public int QuizRoomId { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçersiz email adresi.")]
        public string Email { get; set; }

        public int Score { get; set; }

        public virtual QuizRoom QuizRoom { get; set; }
        [NotMapped] // Veritabanında saklanmayacak
        public string ConnectionId { get; set; }

    }
}