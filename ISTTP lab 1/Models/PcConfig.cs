using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ISTTP_lab_1.Models;

public partial class PcConfig
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Оберіть користувача.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Оберіть процесор.")]
    public int CpuId { get; set; }

    [Required(ErrorMessage = "Оберіть відеокарту.")]
    public int GpuId { get; set; }

    [Required(ErrorMessage = "Вкажіть об'єм оперативної пам'яті.")]
    [Range(1, 256, ErrorMessage = "Оперативної пам'яті має бути від 1 до 256 ГБ.")]
    public int RamGb { get; set; }

    [Required(ErrorMessage = "Оберіть операційну систему.")]
    public OsEnum Os { get; set; }

    [ValidateNever]
    public virtual Cpu Cpu { get; set; } = null!;

    [ValidateNever]
    public virtual Gpu Gpu { get; set; } = null!;

    [ValidateNever]
    public virtual User User { get; set; } = null!;
}
