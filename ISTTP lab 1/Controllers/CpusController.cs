using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ISTTP_lab_1.Data;
using ISTTP_lab_1.Models;
using Microsoft.AspNetCore.Authorization;

namespace ISTTP_lab_1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CpusController : Controller
    {
        private readonly AppDbContext _context;

        public CpusController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Cpus
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cpus.ToListAsync());
        }

        // GET: Cpus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cpu = await _context.Cpus
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cpu == null)
            {
                return NotFound();
            }

            return View(cpu);
        }

        // GET: Cpus/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cpus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ModelName,BenchmarkScore,CoresNumber")] Cpu cpu)
        {
            if (_context.Cpus.Any(c => c.ModelName == cpu.ModelName))
            {
                ModelState.AddModelError("ModelName", "Процесор з такою назвою вже існує! Будь ласка, вкажіть іншу.");
            }

            if (ModelState.IsValid)
            {
                try
                { 
                    _context.Add(cpu);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Сталася помилка збереження в базу даних.");
                }
            }
            return View(cpu);
        }

        // GET: Cpus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cpu = await _context.Cpus.FindAsync(id);
            if (cpu == null)
            {
                return NotFound();
            }
            return View(cpu);
        }

        // POST: Cpus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ModelName,BenchmarkScore,CoresNumber")] Cpu cpu)
        {
            if (id != cpu.Id)
            {
                return NotFound();
            }

            if (_context.Cpus.Any(c => c.ModelName == cpu.ModelName && c.Id != cpu.Id))
            {
                ModelState.AddModelError("ModelName", "Процесор з такою назвою вже існує! Будь ласка, вкажіть іншу.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cpu);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CpuExists(cpu.Id))
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
            return View(cpu);
        }

        // GET: Cpus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cpu = await _context.Cpus
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cpu == null)
            {
                return NotFound();
            }

            return View(cpu);
        }

        // POST: Cpus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cpu = await _context.Cpus.FindAsync(id);
            if (cpu != null)
            {
                try
                {
                    _context.Cpus.Remove(cpu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = $"Неможливо видалити процесор '{cpu.ModelName}', оскільки він використовується у збірках ПК або вимогах до ігор!";
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CpuExists(int id)
        {
            return _context.Cpus.Any(e => e.Id == id);
        }
    }
}
