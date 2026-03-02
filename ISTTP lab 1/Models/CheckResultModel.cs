namespace ISTTP_lab_1.Models
{
    public class RequirementCheckResult
    {
        public Requirement Requirement { get; set; } = null!;
        public bool CpuOk { get; set; }
        public bool GpuOk { get; set; }
        public bool RamOk { get; set; }
        public bool OsOk { get; set; }

        // Загальний результат рахується автоматично
        public bool AllOk => CpuOk && GpuOk && RamOk && OsOk;
    }
    public class CheckResultModel
    {
        public PcConfig Pc { get; set; } = null!;
        public Game Game { get; set; } = null!;
        public List<RequirementCheckResult> Results { get; set; } = new List<RequirementCheckResult>();
    }
}
