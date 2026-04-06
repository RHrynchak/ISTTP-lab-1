using ClosedXML.Excel;
using ISTTP_lab_1.Data;
using ISTTP_lab_1.Models;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportData()
        {
            var cpus = await _context.Cpus.ToListAsync();
            var gpus = await _context.Gpus.ToListAsync();
            var games = await _context.Games.ToListAsync();
            var users = await _context.Users.ToListAsync();
            var requirements = await _context.Requirements
                .Include(r => r.Game).Include(r => r.Cpu).Include(r => r.Gpu)
                .ToListAsync();
            var pcConfigs = await _context.PcConfigs
                .Include(p => p.User).Include(p => p.Cpu).Include(p => p.Gpu)
                .ToListAsync();

            using var workbook = new XLWorkbook();

            ExportCpu(workbook, cpus);
            ExportGpu(workbook, gpus);
            ExportGame(workbook, games);
            ExportUser(workbook, users);
            ExportRequirements(workbook, requirements);
            ExportPcConfig(workbook, pcConfigs);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "PC_Database_Export.xlsx");
        }

        private static void ExportCpu(XLWorkbook wb, IEnumerable<Cpu> cpus)
        {
            var ws = wb.Worksheets.Add("Процесори");
            ws.Cell(1, 1).Value = "ID";
            ws.Cell(1, 2).Value = "Модель процесора";
            ws.Cell(1, 3).Value = "Бенчмарк";
            ws.Cell(1, 4).Value = "Кількість ядер";
            ws.Row(1).Style.Font.Bold = true;

            int row = 1;
            foreach (var cpu in cpus)
            {
                row++;
                ws.Cell(row, 1).Value = cpu.Id;
                ws.Cell(row, 2).Value = cpu.ModelName;
                ws.Cell(row, 3).Value = cpu.BenchmarkScore;
                ws.Cell(row, 4).Value = cpu.CoresNumber;
            }
            ws.Columns().AdjustToContents();
        }

        private static void ExportGpu(XLWorkbook wb, IEnumerable<Gpu> gpus)
        {
            var ws = wb.Worksheets.Add("Відеокарти");
            ws.Cell(1, 1).Value = "ID";
            ws.Cell(1, 2).Value = "Модель відеокарти";
            ws.Cell(1, 3).Value = "Бенчмарк";
            ws.Cell(1, 4).Value = "VRAM (ГБ)";
            ws.Row(1).Style.Font.Bold = true;

            int row = 1;
            foreach (var gpu in gpus)
            {
                row++;
                ws.Cell(row, 1).Value = gpu.Id;
                ws.Cell(row, 2).Value = gpu.ModelName;
                ws.Cell(row, 3).Value = gpu.BenchmarkScore;
                ws.Cell(row, 4).Value = gpu.VramGb;
            }
            ws.Columns().AdjustToContents();
        }

        private static void ExportGame(XLWorkbook wb, IEnumerable<Game> games)
        {
            var ws = wb.Worksheets.Add("Ігри");
            ws.Cell(1, 1).Value = "ID";
            ws.Cell(1, 2).Value = "Назва";
            ws.Cell(1, 3).Value = "Дата виходу";
            ws.Cell(1, 4).Value = "Розмір (ГБ)";
            ws.Row(1).Style.Font.Bold = true;

            int row = 1;
            foreach (var game in games)
            {
                row++;
                ws.Cell(row, 1).Value = game.Id;
                ws.Cell(row, 2).Value = game.Title;
                ws.Cell(row, 3).Value = game.ReleaseDate.ToDateTime(TimeOnly.MinValue);
                ws.Cell(row, 3).Style.DateFormat.Format = "dd.MM.yyyy";
                ws.Cell(row, 4).Value = game.SizeGb;
            }
            ws.Columns().AdjustToContents();
        }

        private static void ExportUser(XLWorkbook wb, IEnumerable<User> users)
        {
            var ws = wb.Worksheets.Add("Користувачі"); // БЕЗ ХЕШІВ
            ws.Cell(1, 1).Value = "ID";
            ws.Cell(1, 2).Value = "Логін";
            ws.Cell(1, 3).Value = "Email";
            ws.Row(1).Style.Font.Bold = true;

            int row = 1;
            foreach (var user in users)
            {
                row++;
                ws.Cell(row, 1).Value = user.Id;
                ws.Cell(row, 2).Value = user.Username;
                ws.Cell(row, 3).Value = user.Email;
            }
            ws.Columns().AdjustToContents();
        }

        private static void ExportRequirements(XLWorkbook wb, IEnumerable<Requirement> requirements)
        {
            var ws = wb.Worksheets.Add("Вимоги");
            ws.Cell(1, 1).Value = "ID";
            ws.Cell(1, 2).Value = "Гра (Назва)";
            ws.Cell(1, 3).Value = "Процесор (Модель)";
            ws.Cell(1, 4).Value = "Відеокарта (Модель)";
            ws.Cell(1, 5).Value = "Тип вимог";
            ws.Cell(1, 6).Value = "RAM (ГБ)";
            ws.Cell(1, 7).Value = "VRAM (ГБ)";
            ws.Cell(1, 8).Value = "Ядра CPU";
            ws.Cell(1, 9).Value = "Підтримувані ОС";
            ws.Row(1).Style.Font.Bold = true;

            int row = 1;
            foreach (var req in requirements)
            {
                row++;
                ws.Cell(row, 1).Value = req.Id;
                ws.Cell(row, 2).Value = req.Game?.Title;
                ws.Cell(row, 3).Value = req.Cpu?.ModelName;
                ws.Cell(row, 4).Value = req.Gpu?.ModelName;
                ws.Cell(row, 5).Value = req.Type.ToString();
                ws.Cell(row, 6).Value = req.RamGb;
                ws.Cell(row, 7).Value = req.VramGb;
                ws.Cell(row, 8).Value = req.CpuCores;
                if (req.OSes != null && req.OSes.Any())
                    ws.Cell(row, 9).Value = string.Join(", ", req.OSes);
            }
            ws.Columns().AdjustToContents();
        }

        private static void ExportPcConfig(XLWorkbook wb, IEnumerable<PcConfig> pcConfigs)
        {
            var ws = wb.Worksheets.Add("Збірки ПК");
            ws.Cell(1, 1).Value = "ID";
            ws.Cell(1, 2).Value = "Користувач (Логін)";
            ws.Cell(1, 3).Value = "Процесор (Модель)";
            ws.Cell(1, 4).Value = "Відеокарта (Модель)";
            ws.Cell(1, 5).Value = "RAM (ГБ)";
            ws.Cell(1, 6).Value = "Операційна система";
            ws.Row(1).Style.Font.Bold = true;

            int row = 1;
            foreach (var pc in pcConfigs)
            {
                row++;
                ws.Cell(row, 1).Value = pc.Id;
                ws.Cell(row, 2).Value = pc.User?.Username;
                ws.Cell(row, 3).Value = pc.Cpu?.ModelName;
                ws.Cell(row, 4).Value = pc.Gpu?.ModelName;
                ws.Cell(row, 5).Value = pc.RamGb;
                ws.Cell(row, 6).Value = pc.Os.ToString();
            }
            ws.Columns().AdjustToContents();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportData(IFormFile fileExcel)
        {
            if (fileExcel == null || fileExcel.Length == 0)
                return RedirectToAction(nameof(Index));

            var importErrors = new List<string>();

            using var stream = new MemoryStream();
            await fileExcel.CopyToAsync(stream);
            using var workbook = new XLWorkbook(stream);

            await ImportCpu(workbook, importErrors);
            await ImportGpu(workbook, importErrors);
            await ImportGame(workbook, importErrors);
            await ImportUser(workbook, importErrors);

            //Зберігаємо цп/відеокарти/ігри/користувачів щоб використовувати їх при імпорті вимог/збірок
            await _context.SaveChangesAsync();

            await ImportRequirements(workbook, importErrors);
            await ImportPcConfig(workbook, importErrors);

            await _context.SaveChangesAsync();

            if (importErrors.Any())
            {
                TempData["ImportWarnings"] = string.Join("<br/>", importErrors);
            }
            else
            {
                TempData["ImportSuccess"] = "Усі дані успішно імпортовано та оновлено!";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task ImportCpu(XLWorkbook workbook, List<string> errors)
        {
            if (!workbook.TryGetWorksheet("Процесори", out IXLWorksheet ws)) return;
            var usedRange = ws.RangeUsed();
            if (usedRange == null) return;

            foreach (var row in usedRange.RowsUsed().Skip(1))
            {
                int rowNum = row.RowNumber();
                if (row.Cell(2).IsEmpty() || row.Cell(3).IsEmpty() || row.Cell(4).IsEmpty())
                {
                    errors.Add($"[Процесори] Рядок {rowNum}: пропущено (порожні обов'язкові клітинки).");
                    continue;
                }

                string name = row.Cell(2).GetString().Trim();
                if (!row.Cell(3).TryGetValue<int>(out int score) || !row.Cell(4).TryGetValue<int>(out int cores))
                {
                    errors.Add($"[Процесори] Рядок {rowNum}: пропущено (бенчмарк або кількість ядер не є числом).");
                    continue;
                }
                if (name.Length > 100 || score < 1 || score > 150000 || cores < 1 || cores > 256)
                {
                    errors.Add($"[Процесори] Рядок {rowNum}: пропущено (дані виходять за допустимі межі).");
                    continue;
                }

                Cpu? existing = null;
                if (row.Cell(1).TryGetValue<int>(out int id) && id > 0)
                    existing = await _context.Cpus.FindAsync(id);

                bool isDuplicateInDb = existing != null
                    ? await _context.Cpus.AnyAsync(c => c.Id != existing.Id && c.ModelName == name)
                    : await _context.Cpus.AnyAsync(c => c.ModelName == name);
                bool isDuplicateInLocal = existing != null
                    ? _context.Cpus.Local.Any(c => c.Id != existing.Id && c.ModelName == name)
                    : _context.Cpus.Local.Any(c => c.ModelName == name);

                if (isDuplicateInDb || isDuplicateInLocal)
                {
                    errors.Add($"[Процесори] Рядок {rowNum}: пропущено (модель '{name}' вже існує або дублюється у файлі).");
                    continue;
                }

                if (existing != null)
                {
                    existing.ModelName = name;
                    existing.BenchmarkScore = score;
                    existing.CoresNumber = cores;
                }
                else
                {
                    _context.Cpus.Add(new Cpu { ModelName = name, BenchmarkScore = score, CoresNumber = cores });
                }
            }
        }

        private async Task ImportGpu(XLWorkbook workbook, List<string> errors)
        {
            if (!workbook.TryGetWorksheet("Відеокарти", out IXLWorksheet ws)) return;
            var usedRange = ws.RangeUsed();
            if (usedRange == null) return;

            foreach (var row in usedRange.RowsUsed().Skip(1))
            {
                int rowNum = row.RowNumber();
                if (row.Cell(2).IsEmpty() || row.Cell(3).IsEmpty() || row.Cell(4).IsEmpty())
                {
                    errors.Add($"[Відеокарти] Рядок {rowNum}: пропущено (порожні обов'язкові клітинки).");
                    continue;
                }

                string name = row.Cell(2).GetString().Trim();
                if (!row.Cell(3).TryGetValue<int>(out int score) || !row.Cell(4).TryGetValue<int>(out int vram))
                {
                    errors.Add($"[Відеокарти] Рядок {rowNum}: пропущено (бенчмарк або vram не є числом).");
                    continue;
                }
                if (name.Length > 100 || score < 1 || score > 150000 || vram < 1 || vram > 64)
                {
                    errors.Add($"[Відеокарти] Рядок {rowNum}: пропущено (дані виходять за допустимі межі).");
                    continue;
                }

                Gpu? existing = null;
                if (row.Cell(1).TryGetValue<int>(out int id) && id > 0)
                    existing = await _context.Gpus.FindAsync(id);

                bool isDuplicateInDb = existing != null
                    ? await _context.Gpus.AnyAsync(g => g.Id != existing.Id && g.ModelName == name)
                    : await _context.Gpus.AnyAsync(g => g.ModelName == name);
                bool isDuplicateInLocal = existing != null
                    ? _context.Gpus.Local.Any(g => g.Id != existing.Id && g.ModelName == name)
                    : _context.Gpus.Local.Any(g => g.ModelName == name);

                if (isDuplicateInDb || isDuplicateInLocal)
                {
                    errors.Add($"[Відеокарти] Рядок {rowNum}: пропущено (модель '{name}' вже існує або дублюється у файлі).");
                    continue;
                }

                if (existing != null)
                {
                    existing.ModelName = name;
                    existing.BenchmarkScore = score;
                    existing.VramGb = vram;
                }
                else
                {
                    _context.Gpus.Add(new Gpu { ModelName = name, BenchmarkScore = score, VramGb = vram });
                }
            }
        }

        private async Task ImportGame(XLWorkbook workbook, List<string> errors)
        {
            if (!workbook.TryGetWorksheet("Ігри", out IXLWorksheet ws)) return;
            var usedRange = ws.RangeUsed();
            if (usedRange == null) return;

            foreach (var row in usedRange.RowsUsed().Skip(1))
            {
                int rowNum = row.RowNumber();
                if (row.Cell(2).IsEmpty() || row.Cell(3).IsEmpty() || row.Cell(4).IsEmpty())
                {
                    errors.Add($"[Ігри] Рядок {rowNum}: пропущено (порожні обов'язкові клітинки).");
                    continue;
                }

                string title = row.Cell(2).GetString().Trim();
                if (!row.Cell(3).TryGetValue<DateTime>(out DateTime releaseDate) || !row.Cell(4).TryGetValue<decimal>(out decimal size))
                {
                    errors.Add($"[Ігри] Рядок {rowNum}: пропущено (некоректний формат дати або розміру гри).");
                    continue;
                }
                if (title.Length > 150 || size < 0.1m || size > 2000.0m)
                {
                    errors.Add($"[Ігри] Рядок {rowNum}: пропущено (задовга назва або завеликий/замалий розмір гри).");
                    continue;
                }

                Game? existing = null;
                if (row.Cell(1).TryGetValue<int>(out int id) && id > 0)
                    existing = await _context.Games.FindAsync(id);

                bool isDuplicateInDb = existing != null
                    ? await _context.Games.AnyAsync(g => g.Id != existing.Id && g.Title == title)
                    : await _context.Games.AnyAsync(g => g.Title == title);
                bool isDuplicateInLocal = existing != null
                    ? _context.Games.Local.Any(g => g.Id != existing.Id && g.Title == title)
                    : _context.Games.Local.Any(g => g.Title == title);

                if (isDuplicateInDb || isDuplicateInLocal)
                {
                    errors.Add($"[Ігри] Рядок {rowNum}: пропущено (гра з назвою '{title}' вже існує або дублюється у файлі).");
                    continue;
                }

                if (existing != null)
                {
                    existing.Title = title;
                    existing.ReleaseDate = DateOnly.FromDateTime(releaseDate);
                    existing.SizeGb = size;
                }
                else
                {
                    _context.Games.Add(new Game { Title = title, ReleaseDate = DateOnly.FromDateTime(releaseDate), SizeGb = size });
                }
            }
        }

        private async Task ImportUser(XLWorkbook workbook, List<string> errors)
        {
            if (!workbook.TryGetWorksheet("Користувачі", out IXLWorksheet ws)) return;
            var usedRange = ws.RangeUsed();
            if (usedRange == null) return;

            var emailValidator = new EmailAddressAttribute();

            foreach (var row in usedRange.RowsUsed().Skip(1))
            {
                int rowNum = row.RowNumber();
                if (row.Cell(2).IsEmpty())
                {
                    errors.Add($"[Користувачі] Рядок {rowNum}: пропущено (немає логіна).");
                    continue;
                }

                string username = row.Cell(2).GetString().Trim();
                string? email = row.Cell(3).IsEmpty() ? null : row.Cell(3).GetString().Trim();
                if (string.IsNullOrWhiteSpace(email)) email = null;
                if (username.Length < 3 || username.Length > 50)
                {
                    errors.Add($"[Користувачі] Рядок {rowNum}: пропущено (логін має бути від 3 до 50 символів).");
                    continue;
                }
                if (email != null && !emailValidator.IsValid(email))
                {
                    errors.Add($"[Користувачі] Рядок {rowNum}: пропущено (некоректний формат Email '{email}').");
                    continue;
                }

                User? existing = null;
                if (row.Cell(1).TryGetValue<int>(out int id) && id > 0)
                    existing = await _context.Users.FindAsync(id);

                bool isDuplicateInDb = existing != null
                    ? await _context.Users.AnyAsync(u => u.Id != existing.Id && (u.Username == username || (email != null && u.Email == email)))
                    : await _context.Users.AnyAsync(u => u.Username == username || (email != null && u.Email == email));
                bool isDuplicateInLocal = existing != null
                    ? _context.Users.Local.Any(u => u.Id != existing.Id && (u.Username == username || (email != null && u.Email == email)))
                    : _context.Users.Local.Any(u => u.Username == username || (email != null && u.Email == email));

                if (isDuplicateInDb || isDuplicateInLocal)
                {
                    errors.Add($"[Користувачі] Рядок {rowNum}: пропущено (логін або пошта вже зайняті, або дублюються у файлі).");
                    continue;
                }

                if (existing != null)
                {
                    existing.Username = username;
                    existing.Email = email;
                }
                else
                {
                    _context.Users.Add(new User { Username = username, Email = email, PasswordHash = null });
                }
            }
        }

        private async Task ImportRequirements(XLWorkbook workbook, List<string> errors)
        {
            if (!workbook.TryGetWorksheet("Вимоги", out IXLWorksheet ws)) return;
            var usedRange = ws.RangeUsed();
            if (usedRange == null) return;

            foreach (var row in usedRange.RowsUsed().Skip(1))
            {
                int rowNum = row.RowNumber();
                if (row.Cell(2).IsEmpty() || row.Cell(3).IsEmpty() || row.Cell(4).IsEmpty() ||
                    row.Cell(5).IsEmpty() || row.Cell(6).IsEmpty() || row.Cell(9).IsEmpty())
                {
                    errors.Add($"[Вимоги] Рядок {rowNum}: пропущено (не всі обов'язкові поля заповнені).");
                    continue;
                }

                string gameTitle = row.Cell(2).GetString().Trim();
                string cpuName = row.Cell(3).GetString().Trim();
                string gpuName = row.Cell(4).GetString().Trim();

                var game = await _context.Games.FirstOrDefaultAsync(g => g.Title == gameTitle);
                var cpu = await _context.Cpus.FirstOrDefaultAsync(c => c.ModelName == cpuName);
                var gpu = await _context.Gpus.FirstOrDefaultAsync(g => g.ModelName == gpuName);
                if (game == null || cpu == null || gpu == null)
                {
                    errors.Add($"[Вимоги] Рядок {rowNum}: пропущено (гру, процесор або відеокарту не знайдено в базі).");
                    continue;
                }

                if (!row.Cell(6).TryGetValue<int>(out int ramGb) || ramGb < 1 || ramGb > 256)
                {
                    errors.Add($"[Вимоги] Рядок {rowNum}: пропущено (некоректне значення RAM).");
                    continue;
                }

                string reqTypeStr = row.Cell(5).GetString().Trim();
                if (!Enum.TryParse<RequirementType>(reqTypeStr, true, out var reqType) ||
                    !Enum.IsDefined(typeof(RequirementType), reqType))
                {
                    errors.Add($"[Вимоги] Рядок {rowNum}: пропущено (невідомий тип вимог '{reqTypeStr}').");
                    continue;
                }

                int? vramGb = null;
                if (!row.Cell(7).IsEmpty())
                {
                    if (!row.Cell(7).TryGetValue<int>(out int parsedVram) || parsedVram < 1 || parsedVram > 128)
                    {
                        errors.Add($"[Вимоги] Рядок {rowNum}: пропущено (некоректне значення VRAM).");
                        continue;
                    }
                    vramGb = parsedVram;
                }

                int? cpuCores = null;
                if (!row.Cell(8).IsEmpty())
                {
                    if (!row.Cell(8).TryGetValue<int>(out int parsedCores) || parsedCores < 1 || parsedCores > 256)
                    {
                        errors.Add($"[Вимоги] Рядок {rowNum}: пропущено (некоректна кількість ядер).");
                        continue;
                    }
                    cpuCores = parsedCores;
                }

                var osList = new List<OsEnum>();
                var osString = row.Cell(9).GetString();
                bool hasInvalidOs = false;
                if (!string.IsNullOrWhiteSpace(osString))
                {
                    foreach (var part in osString.Split(','))
                    {
                        string cleanPart = part.Trim();
                        if (Enum.TryParse<OsEnum>(cleanPart, true, out var parsedOs) &&
                            Enum.IsDefined(typeof(OsEnum), parsedOs))
                        {
                            osList.Add(parsedOs);
                        }
                        else
                        {
                            errors.Add($"[Вимоги] Рядок {rowNum}: пропущено (невідома ОС '{cleanPart}').");
                            hasInvalidOs = true;
                        }
                    }
                }
                if (hasInvalidOs) continue;
                if (osList.Count == 0)
                {
                    errors.Add($"[Вимоги] Рядок {rowNum}: пропущено (не вказано жодної ОС).");
                    continue;
                }

                Requirement? existing = null;
                if (row.Cell(1).TryGetValue<int>(out int id) && id > 0)
                    existing = await _context.Requirements.FindAsync(id);

                bool isDuplicateInDb = existing != null
                    ? await _context.Requirements.AnyAsync(r => r.Id != existing.Id && r.GameId == game.Id && r.Type == reqType)
                    : await _context.Requirements.AnyAsync(r => r.GameId == game.Id && r.Type == reqType);
                bool isDuplicateInLocal = existing != null
                    ? _context.Requirements.Local.Any(r => r.Id != existing.Id && r.GameId == game.Id && r.Type == reqType)
                    : _context.Requirements.Local.Any(r => r.GameId == game.Id && r.Type == reqType);

                if (isDuplicateInDb || isDuplicateInLocal)
                {
                    errors.Add($"[Вимоги] Рядок {rowNum}: пропущено (вимоги типу '{reqTypeStr}' для гри '{gameTitle}' вже існують або дублюються у файлі).");
                    continue;
                }

                if (existing != null)
                {
                    existing.GameId = game.Id;
                    existing.CpuId = cpu.Id;
                    existing.GpuId = gpu.Id;
                    existing.Type = reqType;
                    existing.RamGb = ramGb;
                    existing.VramGb = vramGb;
                    existing.CpuCores = cpuCores;
                    existing.OSes = osList;
                }
                else
                {
                    _context.Requirements.Add(new Requirement
                    {
                        GameId = game.Id,
                        CpuId = cpu.Id,
                        GpuId = gpu.Id,
                        Type = reqType,
                        RamGb = ramGb,
                        VramGb = vramGb,
                        CpuCores = cpuCores,
                        OSes = osList
                    });
                }
            }
        }

        private async Task ImportPcConfig(XLWorkbook workbook, List<string> errors)
        {
            if (!workbook.TryGetWorksheet("Збірки ПК", out IXLWorksheet ws)) return;
            var usedRange = ws.RangeUsed();
            if (usedRange == null) return;

            foreach (var row in usedRange.RowsUsed().Skip(1))
            {
                int rowNum = row.RowNumber();
                if (row.Cell(2).IsEmpty() || row.Cell(3).IsEmpty() ||
                    row.Cell(4).IsEmpty() || row.Cell(5).IsEmpty() || row.Cell(6).IsEmpty())
                {
                    errors.Add($"[Збірки ПК] Рядок {rowNum}: пропущено (не всі обов'язкові поля заповнені).");
                    continue;
                }

                string username = row.Cell(2).GetString().Trim();
                string cpuName = row.Cell(3).GetString().Trim();
                string gpuName = row.Cell(4).GetString().Trim();

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                var cpu = await _context.Cpus.FirstOrDefaultAsync(c => c.ModelName == cpuName);
                var gpu = await _context.Gpus.FirstOrDefaultAsync(g => g.ModelName == gpuName);
                if (user == null || cpu == null || gpu == null)
                {
                    errors.Add($"[Збірки ПК] Рядок {rowNum}: пропущено (потрібного користувача, процесор або відеокарту не знайдено).");
                    continue;
                }

                if (!row.Cell(5).TryGetValue<int>(out int ramGb) || ramGb < 1 || ramGb > 256)
                {
                    errors.Add($"[Збірки ПК] Рядок {rowNum}: пропущено (некоректне значення RAM).");
                    continue;
                }

                string osStr = row.Cell(6).GetString().Trim();
                if (!Enum.TryParse<OsEnum>(osStr, true, out var pcOs) ||
                    !Enum.IsDefined(typeof(OsEnum), pcOs))
                {
                    errors.Add($"[Збірки ПК] Рядок {rowNum}: пропущено (невідома ОС '{osStr}').");
                    continue;
                }

                PcConfig? existing = null;
                if (row.Cell(1).TryGetValue<int>(out int id) && id > 0)
                    existing = await _context.PcConfigs.FindAsync(id);

                bool isDuplicateInDb = existing != null
                    ? await _context.PcConfigs.AnyAsync(p => p.Id != existing.Id && p.UserId == user.Id && p.CpuId == cpu.Id && p.GpuId == gpu.Id && p.RamGb == ramGb && p.Os == pcOs)
                    : await _context.PcConfigs.AnyAsync(p => p.UserId == user.Id && p.CpuId == cpu.Id && p.GpuId == gpu.Id && p.RamGb == ramGb && p.Os == pcOs);
                bool isDuplicateInLocal = existing != null
                    ? _context.PcConfigs.Local.Any(p => p.Id != existing.Id && p.UserId == user.Id && p.CpuId == cpu.Id && p.GpuId == gpu.Id && p.RamGb == ramGb && p.Os == pcOs)
                    : _context.PcConfigs.Local.Any(p => p.UserId == user.Id && p.CpuId == cpu.Id && p.GpuId == gpu.Id && p.RamGb == ramGb && p.Os == pcOs);

                if (isDuplicateInDb || isDuplicateInLocal)
                {
                    errors.Add($"[Збірки ПК] Рядок {rowNum}: пропущено (така збірка для користувача '{username}' вже існує або дублюється у файлі).");
                    continue;
                }

                if (existing != null)
                {
                    existing.UserId = user.Id;
                    existing.CpuId = cpu.Id;
                    existing.GpuId = gpu.Id;
                    existing.RamGb = ramGb;
                    existing.Os = pcOs;
                }
                else
                {
                    _context.PcConfigs.Add(new PcConfig
                    {
                        UserId = user.Id,
                        CpuId = cpu.Id,
                        GpuId = gpu.Id,
                        RamGb = ramGb,
                        Os = pcOs
                    });
                }
            }
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