using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizUygulamasi.Models
{
    public class Quiz
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Quiz adı gereklidir.")]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ValidateNever]
        public virtual ICollection<Question> Questions { get; set; }
    }
}