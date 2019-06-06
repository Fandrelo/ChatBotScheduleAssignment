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
    public class EmailsController : Controller
    {
        private readonly ChatBotContext _context;

        public EmailsController(ChatBotContext context)
        {
            _context = context;
        }

        // GET: Emails
        public async Task<IActionResult> Index(long? id)
        {
            var chatBotContext = _context.Emails.Include(e => e.MailListNavigation);
            if(id == null)
            {
                return View(await chatBotContext.ToListAsync());
            }
            else if (_context.MailLists.Any(e => e.Id == id))
            {
                return View(await chatBotContext.Where(e => e.MailList == id).ToListAsync());
            }
            return NotFound();
        }

        // GET: Emails/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var email = await _context.Emails
                .Include(e => e.MailListNavigation)
                .FirstOrDefaultAsync(m => m.Address == id);
            if (email == null)
            {
                return NotFound();
            }

            return View(email);
        }

        // GET: Emails/Create
        public IActionResult Create()
        {
            ViewData["MailList"] = new SelectList(_context.MailLists, "Id", "Name");
            return View();
        }

        // POST: Emails/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Address,MailList")] Email email)
        {
            if (ModelState.IsValid)
            {
                email.OwnerEmail = User.FindFirst(ClaimTypes.Name).Value;
                _context.Add(email);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MailList"] = new SelectList(_context.MailLists, "Id", "Name", email.MailList);
            return View(email);
        }

        // GET: Emails/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var email = await _context.Emails.FindAsync(id);
            if (email == null)
            {
                return NotFound();
            }
            ViewData["MailList"] = new SelectList(_context.MailLists, "Id", "Name", email.MailList);
            return View(email);
        }

        // POST: Emails/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Address,MailList,OwnerEmail")] Email email)
        {
            if (id != email.Address)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(email);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmailExists(email.Address))
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
            ViewData["MailList"] = new SelectList(_context.MailLists, "Id", "Name", email.MailList);
            return View(email);
        }

        // GET: Emails/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var email = await _context.Emails
                .Include(e => e.MailListNavigation)
                .FirstOrDefaultAsync(m => m.Address == id);
            if (email == null)
            {
                return NotFound();
            }

            return View(email);
        }

        // POST: Emails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var email = await _context.Emails.FindAsync(id);
            _context.Emails.Remove(email);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmailExists(string id)
        {
            return _context.Emails.Any(e => e.Address == id);
        }
    }
}
