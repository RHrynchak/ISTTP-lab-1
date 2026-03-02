using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ISTTP_lab_1.Models;

public partial class Requirement
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Оберіть гру.")]
    public int GameId { get; set; }

    [Required(ErrorMessage = "Оберіть процесор.")]
    public int CpuId { get; set; }

    [Required(ErrorMessage = "Оберіть відеокарту.")]
    public int GpuId { get; set; }

    [Required(ErrorMessage = "Оберіть хоча б одну операційну систему.")]
    public List<OsEnum> OSes { get; set; } = new List<OsEnum>();

    [Required(ErrorMessage = "Оберіть тип вимог (Мінімальні/Рекомендовані).")]
    public RequirementType Type { get; set; }

    [Range(1, 128, ErrorMessage = "Обсяг відеопам'яті має бути від 1 до 128 ГБ.")]
    public int? VramGb { get; set; }

    [Range(1, 256, ErrorMessage = "Кількість ядер має бути від 1 до 256.")]
    public int? CpuCores { get; set; }

    [Required(ErrorMessage = "Вкажіть об'єм оперативної пам'яті.")]
    [Range(1, 256, ErrorMessage = "Оперативної пам'яті має бути від 1 до 256 ГБ.")]
    public int RamGb { get; set; }

    [ValidateNever]
    public virtual Cpu Cpu { get; set; } = null!;

    [ValidateNever]
    public virtual Game Game { get; set; } = null!;

    [ValidateNever]
    public virtual Gpu Gpu { get; set; } = null!;
}
