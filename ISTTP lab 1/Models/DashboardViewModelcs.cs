namespace ISTTP_lab_1.Models
{
    public class DashboardViewModel
    {
        public int TotalGames { get; set; }
        public int TotalUsers { get; set; }
        public int TotalPcConfigs { get; set; }
        public List<string> OsLabels { get; set; } = new List<string>();
        public List<int> OsCounts { get; set; } = new List<int>();
        public List<string> TopGpuLabels { get; set; } = new List<string>();
        public List<int> TopGpuCounts { get; set; } = new List<int>();
        public List<string> TopCpuLabels { get; set; } = new List<string>();
        public List<int> TopCpuCounts { get; set; } = new List<int>();
    }
}
