using ISTTP_lab_1.Data;
using ISTTP_lab_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ISTTP_lab_1.Controllers
{
    public class CheckerController : Controller
    {
        private readonly AppDbContext _context;

        public CheckerController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var pcConfigs = await _context.PcConfigs
                .Include(p => p.User)
                .Include(p => p.Cpu)
                .Include(p => p.Gpu)
                .ToListAsync();

            var pcList = pcConfigs.Select(p => new
            {
                Id = p.Id,
                DisplayName = $"{p.User.Username}'s PC ({p.Cpu.ModelName} / {p.Gpu.ModelName})"
            });

            ViewBag.PcConfigId = new SelectList(pcList, "Id", "DisplayName");
            ViewBag.GameId = new SelectList(await _context.Games.ToListAsync(), "Id", "Title");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Result(int pcConfigId, int gameId)
        {
            var pc = await _context.PcConfigs
                .Include(p => p.Cpu)
                .Include(p => p.Gpu)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == pcConfigId);

            var game = await _context.Games
                .Include(g => g.Requirements).ThenInclude(r => r.Cpu)
                .Include(g => g.Requirements).ThenInclude(r => r.Gpu)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (pc == null || game == null) return NotFound();

            var requirements = game.Requirements.OrderBy(r => r.Type).ToList();

            if (!requirements.Any())
            {
                TempData["ErrorMessage"] = $"Для гри '{game.Title}' ще не додані системні вимоги в базу!";
                return RedirectToAction(nameof(Index));
            }

            var checkResult = new CheckResultModel
            {
                Pc = pc,
                Game = game,
                Results = new List<RequirementCheckResult>()
            };

            foreach (var req in requirements)
            {
                var result = new RequirementCheckResult
                {
                    Requirement = req,
                    CpuOk = pc.Cpu.BenchmarkScore >= req.Cpu.BenchmarkScore && pc.Cpu.CoresNumber >= (req.CpuCores ?? 0),
                    GpuOk = pc.Gpu.BenchmarkScore >= req.Gpu.BenchmarkScore && pc.Gpu.VramGb >= (req.VramGb ?? 0),
                    RamOk = pc.RamGb >= req.RamGb,
                    OsOk = req.OSes != null && req.OSes.Contains(pc.Os)
                };

                checkResult.Results.Add(result);
            }

            return View(checkResult);
        }
    }
}
