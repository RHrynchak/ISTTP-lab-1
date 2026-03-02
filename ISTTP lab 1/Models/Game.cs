using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ISTTP_lab_1.Models;

public partial class Game
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Назва гри є обов'язковою.")]
    [StringLength(150, ErrorMessage = "Назва гри не може бути довшою за 150 символів.")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Дата виходу гри є обов'язковую.")]
    public DateOnly ReleaseDate { get; set; }

    [Required(ErrorMessage = "Вага гри є обов'язковою.")]
    [Range(0.1, 2000.0, ErrorMessage = "Вага гри має бути від 0.1 до 500 ГБ.")]
    public decimal SizeGb { get; set; }

    public virtual ICollection<Requirement> Requirements { get; set; } = new List<Requirement>();
}
