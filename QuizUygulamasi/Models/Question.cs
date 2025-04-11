using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizUygulamasi.Models
{
    public class Question
    {
        public int Id { get; set; }
        public int QuizId { get; set; }

        [Required(ErrorMessage = "Soru metni gereklidir.")]
        public string Text { get; set; }

        [Required(ErrorMessage = "1. Seçenek gereklidir.")]
        public string OptionA { get; set; }

        [Required(ErrorMessage = "2. Seçenek gereklidir.")]
        public string OptionB { get; set; }

        [Required(ErrorMessage = "3. Seçenek gereklidir.")]
        public string OptionC { get; set; }

        [Required(ErrorMessage = "4. Seçenek gereklidir.")]
        public string OptionD { get; set; }

        [Required(ErrorMessage = "Doğru cevap gereklidir.")]
        public string CorrectAnswer { get; set; } // A, B, C veya D
        [ValidateNever]
        public virtual Quiz Quiz { get; set; }
    }
}