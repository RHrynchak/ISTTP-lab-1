using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ISTTP_lab_1.Models;

public partial class Gpu
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Назва моделі є обов'язковою.")]
    [StringLength(100, ErrorMessage = "Назва не може перевищувати 100 символів.")]
    public string ModelName { get; set; } = null!;

    [Required(ErrorMessage = "Оцінка моделі є обов'язковою.")]
    [Range(1, 150000, ErrorMessage = "Оцінка має бути від 1 до 200000.")]
    public int BenchmarkScore { get; set; }

    [Required(ErrorMessage = "Обсяг відеопам'яті є обов'язковим.")]
    [Range(1, 64, ErrorMessage = "Обсяг відеопам'яті має бути від 1 до 64 ГБ.")]
    public int VramGb { get; set; }

    public virtual ICollection<PcConfig> PcConfigs { get; set; } = new List<PcConfig>();

    public virtual ICollection<Requirement> Requirements { get; set; } = new List<Requirement>();
}
