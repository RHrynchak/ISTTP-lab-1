using ISTTP_lab_1.Data;
using ISTTP_lab_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ISTTP_lab_1.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new DashboardViewModel();

            vm.TotalGames = await _context.Games.CountAsync();
            vm.TotalUsers = await _context.Users.CountAsync();
            vm.TotalPcConfigs = await _context.PcConfigs.CountAsync();

            var osData = await _context.PcConfigs
                .GroupBy(p => p.Os)
                .Select(g => new { OsName = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            vm.OsLabels = osData.Select(x => x.OsName).ToList();
            vm.OsCounts = osData.Select(x => x.Count).ToList();

            var gpuData = await _context.PcConfigs
                .Include(p => p.Gpu)
                .GroupBy(p => p.Gpu.ModelName)
                .Select(g => new { GpuName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            vm.TopGpuLabels = gpuData.Select(x => x.GpuName).ToList();
            vm.TopGpuCounts = gpuData.Select(x => x.Count).ToList();

            var cpuData = await _context.PcConfigs
                .Include(p => p.Cpu)
                .GroupBy(p => p.Cpu.ModelName)
                .Select(g => new { CpuName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            vm.TopCpuLabels = cpuData.Select(x => x.CpuName).ToList();
            vm.TopCpuCounts = cpuData.Select(x => x.Count).ToList();

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
