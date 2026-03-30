using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ISTTP_lab_1.Data;
using ISTTP_lab_1.Models;

namespace ISTTP_lab_1.Controllers
{
    public class PcConfigsController : Controller
    {
        private readonly AppDbContext _context;

        public PcConfigsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PcConfigs
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.PcConfigs.Include(p => p.Cpu).Include(p => p.Gpu).Include(p => p.User);
            return View(await appDbContext.ToListAsync());
        }

        // GET: PcConfigs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pcConfig = await _context.PcConfigs
                .Include(p => p.Cpu)
                .Include(p => p.Gpu)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pcConfig == null)
            {
                return NotFound();
            }

            return View(pcConfig);
        }

        // GET: PcConfigs/Create
        public IActionResult Create()
        {
            ViewData["CpuId"] = new SelectList(_context.Cpus, "Id", "ModelName");
            ViewData["GpuId"] = new SelectList(_context.Gpus, "Id", "ModelName");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username");
            return View();
        }

        // POST: PcConfigs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,CpuId,GpuId,RamGb,Os")] PcConfig pcConfig)
        {
            if (_context.PcConfigs.Any(p =>
                p.UserId == pcConfig.UserId &&
                p.CpuId == pcConfig.CpuId &&
                p.GpuId == pcConfig.GpuId &&
                p.RamGb == pcConfig.RamGb &&
                p.Os == pcConfig.Os))
            {
                ModelState.AddModelError("", "У цього користувача вже є точно така сама збірка ПК!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(pcConfig);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Сталася помилка збереження в базу даних.");
                }
            }
            ViewData["CpuId"] = new SelectList(_context.Cpus, "Id", "ModelName", pcConfig.CpuId);
            ViewData["GpuId"] = new SelectList(_context.Gpus, "Id", "ModelName", pcConfig.GpuId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", pcConfig.UserId);
            return View(pcConfig);
        }

        // GET: PcConfigs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pcConfig = await _context.PcConfigs.FindAsync(id);
            if (pcConfig == null)
            {
                return NotFound();
            }
            ViewData["CpuId"] = new SelectList(_context.Cpus, "Id", "ModelName");
            ViewData["GpuId"] = new SelectList(_context.Gpus, "Id", "ModelName");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username");
            return View(pcConfig);
        }

        // POST: PcConfigs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,CpuId,GpuId,RamGb,Os")] PcConfig pcConfig)
        {
            if (id != pcConfig.Id)
            {
                return NotFound();
            }

            if (_context.PcConfigs.Any(p =>
                p.Id != pcConfig.Id &&
                p.UserId == pcConfig.UserId &&
                p.CpuId == pcConfig.CpuId &&
                p.GpuId == pcConfig.GpuId &&
                p.RamGb == pcConfig.RamGb &&
                p.Os == pcConfig.Os))
            {
                ModelState.AddModelError("", "У цього користувача вже є точно така сама збірка ПК!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pcConfig);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PcConfigExists(pcConfig.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Сталася помилка при оновленні бази даних.");
                }
            }

            ViewData["CpuId"] = new SelectList(_context.Cpus, "Id", "ModelName", pcConfig.CpuId);
            ViewData["GpuId"] = new SelectList(_context.Gpus, "Id", "ModelName", pcConfig.GpuId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", pcConfig.UserId);
            return View(pcConfig);
        }

        // GET: PcConfigs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pcConfig = await _context.PcConfigs
                .Include(p => p.Cpu)
                .Include(p => p.Gpu)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pcConfig == null)
            {
                return NotFound();
            }

            return View(pcConfig);
        }

        // POST: PcConfigs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pcConfig = await _context.PcConfigs.FindAsync(id);
            if (pcConfig != null)
            {
                _context.PcConfigs.Remove(pcConfig);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PcConfigExists(int id)
        {
            return _context.PcConfigs.Any(e => e.Id == id);
        }
    }
}
