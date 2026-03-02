using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ISTTP_lab_1.Models;

public partial class User
{
    public int Id { get; set; }

    [EmailAddress(ErrorMessage = "Введіть коректну електронну адресу.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Логін є обов'язковим.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Логін має містити від 3 до 50 символів.")]
    public string Username { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public virtual ICollection<PcConfig> PcConfigs { get; set; } = new List<PcConfig>();
}
