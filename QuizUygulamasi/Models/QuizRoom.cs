using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizUygulamasi.Models
{
    public class QuizRoom
    {
        public int Id { get; set; }
        public int QuizId { get; set; }

        [Required(ErrorMessage = "Oda kodu gereklidir.")]
        public string RoomCode { get; set; } = Guid.NewGuid().ToString().Substring(0, 6).ToUpper(); // Rastgele 6 haneli kod

        public bool IsActive { get; set; } = false; // Oyun başladı mı?
        public int CurrentQuestionIndex { get; set; } = 0;
        public DateTime QuestionStartTime { get; set; }
        public int ParticipantCount { get; set; } = 0;

        public virtual Quiz Quiz { get; set; }
        public virtual ICollection<Participant> Participants { get; set; }
    }
}