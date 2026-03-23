using ClosedXML.Excel;
using ISTTP_lab_1.Data;
using ISTTP_lab_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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

        [HttpGet]
        public async Task<IActionResult> ExportData()
        {
            var cpus = await _context.Cpus.ToListAsync();
            var gpus = await _context.Gpus.ToListAsync();
            var games = await _context.Games.ToListAsync();
            var users = await _context.Users.ToListAsync();

            var requirements = await _context.Requirements
                .Include(r => r.Game)
                .Include(r => r.Cpu)
                .Include(r => r.Gpu)
                .ToListAsync();

            var pcConfigs = await _context.PcConfigs
                .Include(p => p.User)
                .Include(p => p.Cpu)
                .Include(p => p.Gpu)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var wsCpu = workbook.Worksheets.Add("Процесори");
                wsCpu.Cell(1, 1).Value = "ID";
                wsCpu.Cell(1, 2).Value = "Модель процесора";
                wsCpu.Cell(1, 3).Value = "Бенчмарк";
                wsCpu.Cell(1, 4).Value = "Кількість ядер";
                wsCpu.Row(1).Style.Font.Bold = true;

                int rowCpu = 1;
                foreach (var cpu in cpus)
                {
                    rowCpu++;
                    wsCpu.Cell(rowCpu, 1).Value = cpu.Id;
                    wsCpu.Cell(rowCpu, 2).Value = cpu.ModelName;
                    wsCpu.Cell(rowCpu, 3).Value = cpu.BenchmarkScore;
                    wsCpu.Cell(rowCpu, 4).Value = cpu.CoresNumber;
                }
                wsCpu.Columns().AdjustToContents();

                var wsGpu = workbook.Worksheets.Add("Відеокарти");
                wsGpu.Cell(1, 1).Value = "ID";
                wsGpu.Cell(1, 2).Value = "Модель відеокарти";
                wsGpu.Cell(1, 3).Value = "Бенчмарк";
                wsGpu.Cell(1, 4).Value = "VRAM (ГБ)";
                wsGpu.Row(1).Style.Font.Bold = true;

                int rowGpu = 1;
                foreach (var gpu in gpus)
                {
                    rowGpu++;
                    wsGpu.Cell(rowGpu, 1).Value = gpu.Id;
                    wsGpu.Cell(rowGpu, 2).Value = gpu.ModelName;
                    wsGpu.Cell(rowGpu, 3).Value = gpu.BenchmarkScore;
                    wsGpu.Cell(rowGpu, 4).Value = gpu.VramGb;
                }
                wsGpu.Columns().AdjustToContents();

                var wsGame = workbook.Worksheets.Add("Ігри");
                wsGame.Cell(1, 1).Value = "ID";
                wsGame.Cell(1, 2).Value = "Назва";
                wsGame.Cell(1, 3).Value = "Дата виходу";
                wsGame.Cell(1, 4).Value = "Розмір (ГБ)";
                wsGame.Row(1).Style.Font.Bold = true;

                int rowGame = 1;
                foreach (var game in games)
                {
                    rowGame++;
                    wsGame.Cell(rowGame, 1).Value = game.Id;
                    wsGame.Cell(rowGame, 2).Value = game.Title;
                    wsGame.Cell(rowGame, 3).Value = game.ReleaseDate.ToDateTime(TimeOnly.MinValue);
                    wsGame.Cell(rowGame, 3).Style.DateFormat.Format = "dd.MM.yyyy";
                    wsGame.Cell(rowGame, 4).Value = game.SizeGb;
                }
                wsGame.Columns().AdjustToContents();

                var wsUser = workbook.Worksheets.Add("Користувачі"); // БЕЗ ХЕШІВ
                wsUser.Cell(1, 1).Value = "ID";
                wsUser.Cell(1, 2).Value = "Логін";
                wsUser.Cell(1, 3).Value = "Email";
                wsUser.Row(1).Style.Font.Bold = true;

                int rowUser = 1;
                foreach (var user in users)
                {
                    rowUser++;
                    wsUser.Cell(rowUser, 1).Value = user.Id;
                    wsUser.Cell(rowUser, 2).Value = user.Username;
                    wsUser.Cell(rowUser, 3).Value = user.Email;
                }
                wsUser.Columns().AdjustToContents();

                var wsReq = workbook.Worksheets.Add("Вимоги");
                wsReq.Cell(1, 1).Value = "ID";
                wsReq.Cell(1, 2).Value = "Гра (Назва)";
                wsReq.Cell(1, 3).Value = "Процесор (Модель)";
                wsReq.Cell(1, 4).Value = "Відеокарта (Модель)";
                wsReq.Cell(1, 5).Value = "Тип вимог";
                wsReq.Cell(1, 6).Value = "RAM (ГБ)";
                wsReq.Cell(1, 7).Value = "VRAM (ГБ)";
                wsReq.Cell(1, 8).Value = "Ядра CPU";
                wsReq.Cell(1, 9).Value = "Підтримувані ОС";
                wsReq.Row(1).Style.Font.Bold = true;

                int rowReq = 1;
                foreach (var req in requirements)
                {
                    rowReq++;
                    wsReq.Cell(rowReq, 1).Value = req.Id;
                    wsReq.Cell(rowReq, 2).Value = req.Game?.Title;
                    wsReq.Cell(rowReq, 3).Value = req.Cpu?.ModelName;
                    wsReq.Cell(rowReq, 4).Value = req.Gpu?.ModelName;
                    wsReq.Cell(rowReq, 5).Value = req.Type.ToString();
                    wsReq.Cell(rowReq, 6).Value = req.RamGb;
                    wsReq.Cell(rowReq, 7).Value = req.VramGb;
                    wsReq.Cell(rowReq, 8).Value = req.CpuCores;
                    if (req.OSes != null && req.OSes.Any())
                    {
                        wsReq.Cell(rowReq, 9).Value = string.Join(", ", req.OSes);
                    }
                }
                wsReq.Columns().AdjustToContents();

                var wsPc = workbook.Worksheets.Add("Збірки ПК");
                wsPc.Cell(1, 1).Value = "ID";
                wsPc.Cell(1, 2).Value = "Користувач (Логін)";
                wsPc.Cell(1, 3).Value = "Процесор (Модель)";
                wsPc.Cell(1, 4).Value = "Відеокарта (Модель)";
                wsPc.Cell(1, 5).Value = "RAM (ГБ)";
                wsPc.Cell(1, 6).Value = "Операційна система";
                wsPc.Row(1).Style.Font.Bold = true;

                int rowPc = 1;
                foreach (var pc in pcConfigs)
                {
                    rowPc++;
                    wsPc.Cell(rowPc, 1).Value = pc.Id;
                    wsPc.Cell(rowPc, 2).Value = pc.User?.Username;
                    wsPc.Cell(rowPc, 3).Value = pc.Cpu?.ModelName;
                    wsPc.Cell(rowPc, 4).Value = pc.Gpu?.ModelName;
                    wsPc.Cell(rowPc, 5).Value = pc.RamGb;
                    wsPc.Cell(rowPc, 6).Value = pc.Os.ToString();
                }
                wsPc.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PC_Database_Export.xlsx");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportData(IFormFile fileExcel)
        {
            if (fileExcel != null && fileExcel.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    await fileExcel.CopyToAsync(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var emailValidator = new EmailAddressAttribute();

                        if (workbook.TryGetWorksheet("Процесори", out IXLWorksheet wsCpu))
                        {
                            var usedRange = wsCpu.RangeUsed();
                            if (usedRange != null)
                            {
                                foreach (var row in usedRange.RowsUsed().Skip(1))
                                {
                                    if (row.Cell(2).IsEmpty() || row.Cell(3).IsEmpty() || row.Cell(4).IsEmpty()) continue;

                                    string name = row.Cell(2).GetValue<string>();
                                    int score = row.Cell(3).GetValue<int>();
                                    int cores = row.Cell(4).GetValue<int>();

                                    if (name.Length > 100 || score < 1 || score > 150000 || cores < 1 || cores > 256) continue;

                                    if (!_context.Cpus.Any(c => c.ModelName == name))
                                        _context.Cpus.Add(new Cpu { ModelName = name, BenchmarkScore = score, CoresNumber = cores });
                                }
                            }
                        }

                        if (workbook.TryGetWorksheet("Відеокарти", out IXLWorksheet wsGpu))
                        {
                            var usedRange = wsGpu.RangeUsed();
                            if (usedRange != null)
                            {
                                foreach (var row in usedRange.RowsUsed().Skip(1))
                                {
                                    if (row.Cell(2).IsEmpty() || row.Cell(3).IsEmpty() || row.Cell(4).IsEmpty()) continue;

                                    string name = row.Cell(2).GetValue<string>();
                                    int score = row.Cell(3).GetValue<int>();
                                    int vram = row.Cell(4).GetValue<int>();

                                    if (name.Length > 100 || score < 1 || score > 150000 || vram < 1 || vram > 64) continue;

                                    if (!_context.Gpus.Any(g => g.ModelName == name))
                                        _context.Gpus.Add(new Gpu { ModelName = name, BenchmarkScore = score, VramGb = vram });
                                }
                            }
                        }

                        if (workbook.TryGetWorksheet("Ігри", out IXLWorksheet wsGame))
                        {
                            var usedRange = wsGame.RangeUsed();
                            if (usedRange != null)
                            {
                                foreach (var row in usedRange.RowsUsed().Skip(1))
                                {
                                    if (row.Cell(2).IsEmpty() || row.Cell(3).IsEmpty() || row.Cell(4).IsEmpty()) continue;

                                    string title = row.Cell(2).GetValue<string>();
                                    decimal size = row.Cell(4).GetValue<decimal>();

                                    if (title.Length > 150 || size < 0.1m || size > 2000.0m) continue;

                                    if (!_context.Games.Any(g => g.Title == title))
                                        _context.Games.Add(new Game { Title = title, ReleaseDate = DateOnly.FromDateTime(row.Cell(3).GetValue<DateTime>()), SizeGb = size });
                                }
                            }
                        }

                        if (workbook.TryGetWorksheet("Користувачі", out IXLWorksheet wsUser))
                        {
                            var usedRange = wsUser.RangeUsed();
                            if (usedRange != null)
                            {
                                foreach (var row in usedRange.RowsUsed().Skip(1))
                                {
                                    if (row.Cell(2).IsEmpty() || row.Cell(3).IsEmpty()) continue;

                                    string username = row.Cell(2).GetValue<string>();
                                    string email = row.Cell(3).GetValue<string>();

                                    if (username.Length < 3 || username.Length > 50) continue;
                                    if (!string.IsNullOrEmpty(email) && !emailValidator.IsValid(email)) continue;

                                    if (!_context.Users.Any(u => u.Username == username || u.Email == email))
                                        _context.Users.Add(new User { Username = username, Email = email, PasswordHash = null });
                                }
                            }
                        }

                        await _context.SaveChangesAsync(); //Щоб могли додавати збірки/вимоги для нових користувачів/ігор/цп/відеокарт

                        if (workbook.TryGetWorksheet("Вимоги", out IXLWorksheet wsReq))
                        {
                            var usedRange = wsReq.RangeUsed();
                            if (usedRange != null)
                            {
                                foreach (var row in usedRange.RowsUsed().Skip(1))
                                {
                                    if (row.Cell(2).IsEmpty() || row.Cell(3).IsEmpty() || row.Cell(4).IsEmpty() ||
                                        row.Cell(5).IsEmpty() || row.Cell(6).IsEmpty() || row.Cell(9).IsEmpty())
                                        continue;

                                    string gameTitle = row.Cell(2).GetValue<string>();
                                    string cpuName = row.Cell(3).GetValue<string>();
                                    string gpuName = row.Cell(4).GetValue<string>();

                                    var game = await _context.Games.FirstOrDefaultAsync(g => g.Title == gameTitle);
                                    var cpu = await _context.Cpus.FirstOrDefaultAsync(c => c.ModelName == cpuName);
                                    var gpu = await _context.Gpus.FirstOrDefaultAsync(g => g.ModelName == gpuName);

                                    if (game == null || cpu == null || gpu == null) continue;

                                    int ramGb = row.Cell(6).GetValue<int>();
                                    int? vramGb = row.Cell(7).IsEmpty() ? null : row.Cell(7).GetValue<int>();
                                    int? cpuCores = row.Cell(8).IsEmpty() ? null : row.Cell(8).GetValue<int>();

                                    if (ramGb < 1 || ramGb > 256) continue;
                                    if (vramGb.HasValue && (vramGb.Value < 1 || vramGb.Value > 128)) continue;
                                    if (cpuCores.HasValue && (cpuCores.Value < 1 || cpuCores.Value > 256)) continue;

                                    var reqType = Enum.Parse<RequirementType>(row.Cell(5).GetValue<string>());

                                    var req = new Requirement
                                    {
                                        GameId = game.Id,
                                        CpuId = cpu.Id,
                                        GpuId = gpu.Id,
                                        Type = reqType,
                                        RamGb = ramGb,
                                        VramGb = vramGb,
                                        CpuCores = cpuCores,
                                        OSes = new List<OsEnum>()
                                    };

                                    var osString = row.Cell(9).GetValue<string>();
                                    if (!string.IsNullOrWhiteSpace(osString))
                                    {
                                        var osParts = osString.Split(',');
                                        foreach (var part in osParts)
                                        {
                                            if (Enum.TryParse<OsEnum>(part.Trim(), out var parsedOs))
                                                req.OSes.Add(parsedOs);
                                        }
                                    }

                                    if (!_context.Requirements.Any(r => r.GameId == req.GameId && r.Type == req.Type))
                                        _context.Requirements.Add(req);
                                }
                            }
                        }

                        if (workbook.TryGetWorksheet("Збірки ПК", out IXLWorksheet wsPc))
                        {
                            var usedRange = wsPc.RangeUsed();
                            if (usedRange != null)
                            {
                                foreach (var row in usedRange.RowsUsed().Skip(1))
                                {
                                    if (row.Cell(2).IsEmpty() || row.Cell(3).IsEmpty() ||
                                        row.Cell(4).IsEmpty() || row.Cell(5).IsEmpty() || row.Cell(6).IsEmpty())
                                        continue;

                                    string username = row.Cell(2).GetValue<string>();
                                    string cpuName = row.Cell(3).GetValue<string>();
                                    string gpuName = row.Cell(4).GetValue<string>();

                                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                                    var cpu = await _context.Cpus.FirstOrDefaultAsync(c => c.ModelName == cpuName);
                                    var gpu = await _context.Gpus.FirstOrDefaultAsync(g => g.ModelName == gpuName);

                                    if (user == null || cpu == null || gpu == null) continue;

                                    int ramGb = row.Cell(5).GetValue<int>();
                                    if (ramGb < 1 || ramGb > 256) continue;

                                    var pcOs = Enum.Parse<ISTTP_lab_1.Models.OsEnum>(row.Cell(6).GetValue<string>());

                                    var pc = new PcConfig
                                    {
                                        UserId = user.Id,
                                        CpuId = cpu.Id,
                                        GpuId = gpu.Id,
                                        RamGb = ramGb,
                                        Os = pcOs
                                    };

                                    bool exists = _context.PcConfigs.Any(p =>
                                        p.UserId == pc.UserId && p.CpuId == pc.CpuId &&
                                        p.GpuId == pc.GpuId && p.RamGb == pc.RamGb && p.Os == pc.Os);

                                    if (!exists)
                                        _context.PcConfigs.Add(pc);
                                }
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                }
            }
            return RedirectToAction(nameof(Index));
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