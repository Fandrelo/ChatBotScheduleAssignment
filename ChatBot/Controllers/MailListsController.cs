using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChatBot.Models;
using ChatBot.Models.ScheduleAssignment;
using System.Security.Claims;

namespace ChatBot.Controllers
{
    public class MailListsController : Controller
    {
        private readonly ChatBotContext _context;

        public MailListsController(ChatBotContext context)
        {
            _context = context;
        }

        // GET: MailLists
        public async Task<IActionResult> Index()
        {
            return View(await _context.MailLists.ToListAsync());
        }

        // GET: MailLists/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mailList = await _context.MailLists
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mailList == null)
            {
                return NotFound();
            }

            return View(mailList);
        }

        // GET: MailLists/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MailLists/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] MailList mailList)
        {
            if (ModelState.IsValid)
            {
                mailList.OwnerEmail = User.FindFirst(ClaimTypes.Name).Value;
                _context.Add(mailList);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mailList);
        }

        // GET: MailLists/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mailList = await _context.MailLists.FindAsync(id);
            if (mailList == null)
            {
                return NotFound();
            }
            return View(mailList);
        }

        // POST: MailLists/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name,OwnerEmail")] MailList mailList)
        {
            if (id != mailList.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mailList);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MailListExists(mailList.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(mailList);
        }

        // GET: MailLists/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mailList = await _context.MailLists
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mailList == null)
            {
                return NotFound();
            }

            return View(mailList);
        }

        // POST: MailLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var mailList = await _context.MailLists.FindAsync(id);
            _context.MailLists.Remove(mailList);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MailListExists(long id)
        {
            return _context.MailLists.Any(e => e.Id == id);
        }
    }
}
