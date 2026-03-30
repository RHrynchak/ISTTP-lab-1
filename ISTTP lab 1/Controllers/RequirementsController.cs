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
    public class RequirementsController : Controller
    {
        private readonly AppDbContext _context;

        public RequirementsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Requirements
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Requirements.Include(r => r.Cpu).Include(r => r.Game).Include(r => r.Gpu);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Requirements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requirement = await _context.Requirements
                .Include(r => r.Cpu)
                .Include(r => r.Game)
                .Include(r => r.Gpu)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (requirement == null)
            {
                return NotFound();
            }

            return View(requirement);
        }

        // GET: Requirements/Create
        public IActionResult Create()
        {
            ViewData["CpuId"] = new SelectList(_context.Cpus, "Id", "ModelName");
            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Title");
            ViewData["GpuId"] = new SelectList(_context.Gpus, "Id", "ModelName");
            return View();
        }

        // POST: Requirements/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GameId,CpuId,GpuId,OSes,Type,VramGb,CpuCores,RamGb")] Requirement requirement)
        {
            if (_context.Requirements.Any(r => r.GameId == requirement.GameId && r.Type == requirement.Type))
            {
                ModelState.AddModelError("Type", "Вимоги такого типу для цієї гри вже існують!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(requirement);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Сталася помилка збереження в базу даних.");
                }
            }

            ViewData["CpuId"] = new SelectList(_context.Cpus, "Id", "ModelName", requirement.CpuId);
            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Title", requirement.GameId);
            ViewData["GpuId"] = new SelectList(_context.Gpus, "Id", "ModelName", requirement.GpuId);
            return View(requirement);
        }

        // GET: Requirements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requirement = await _context.Requirements.FindAsync(id);
            if (requirement == null)
            {
                return NotFound();
            }
            ViewData["CpuId"] = new SelectList(_context.Cpus, "Id", "ModelName");
            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Title");
            ViewData["GpuId"] = new SelectList(_context.Gpus, "Id", "ModelName");
            return View(requirement);
        }

        // POST: Requirements/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GameId,CpuId,GpuId,OSes,Type,VramGb,CpuCores,RamGb")] Requirement requirement)
        {
            if (id != requirement.Id)
            {
                return NotFound();
            }

            if (_context.Requirements.Any(r => r.GameId == requirement.GameId && r.Type == requirement.Type && r.Id != requirement.Id))
            {
                ModelState.AddModelError("Type", "Вимоги такого типу для цієї гри вже існують!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(requirement);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RequirementExists(requirement.Id)) return NotFound();
                    else throw;
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Сталася помилка при оновленні бази даних.");
                }
            }

            ViewData["CpuId"] = new SelectList(_context.Cpus, "Id", "ModelName", requirement.CpuId);
            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Title", requirement.GameId);
            ViewData["GpuId"] = new SelectList(_context.Gpus, "Id", "ModelName", requirement.GpuId);
            return View(requirement);
        }

        // GET: Requirements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requirement = await _context.Requirements
                .Include(r => r.Cpu)
                .Include(r => r.Game)
                .Include(r => r.Gpu)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (requirement == null)
            {
                return NotFound();
            }

            return View(requirement);
        }

        // POST: Requirements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var requirement = await _context.Requirements.FindAsync(id);
            if (requirement != null)
            {
                _context.Requirements.Remove(requirement);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RequirementExists(int id)
        {
            return _context.Requirements.Any(e => e.Id == id);
        }
    }
}
