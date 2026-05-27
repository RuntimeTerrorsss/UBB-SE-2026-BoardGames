// <copyright file="AccountsController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data;
using BoardGames.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoardGames.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly AppDbContext _context;

        public AccountsController(AppDbContext context)
        {
            this._context = context;
        }

        // GET: Accounts
        public async Task<IActionResult> Index()
        {
            return View(await this._context.Users.ToListAsync());
        }

        // GET: Accounts/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var account = await this._context.Users
                .FirstOrDefaultAsync(inputAccount => inputAccount.Id == id);
            if (account == null)
            {
                return this.NotFound();
            }

            return View(account);
        }

        // GET: Accounts/Create
        public IActionResult Create()
        {
            return this.View();
        }

        // POST: Accounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DisplayName,Username,Email,PasswordHash,PhoneNumber,AvatarUrl,IsSuspended,CreatedAt,UpdatedAt,Country,City,StreetName,StreetNumber")] User account)
        {
            if (this.ModelState.IsValid)
            {
                account.Id = Guid.NewGuid();
                this._context.Add(account);
                await this._context.SaveChangesAsync();
                return this.RedirectToAction(nameof(this.Index));
            }

            return View(account);
        }

        // GET: Accounts/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var account = await this._context.Users.FindAsync(id);
            if (account == null)
            {
                return this.NotFound();
            }

            return View(account);
        }

        // POST: Accounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,DisplayName,Username,Email,PasswordHash,PhoneNumber,AvatarUrl,IsSuspended,CreatedAt,UpdatedAt,Country,City,StreetName,StreetNumber")] User account)
        {
            if (id != account.Id)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                try
                {
                    this._context.Update(account);
                    await this._context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.Id))
                    {
                        return this.NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return this.RedirectToAction(nameof(this.Index));
            }

            return View(account);
        }

        // GET: Accounts/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var account = await this._context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return this.NotFound();
            }

            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var account = await this._context.Users.FindAsync(id);
            if (account != null)
            {
                this._context.Users.Remove(account);
            }

            await this._context.SaveChangesAsync();
            return this.RedirectToAction(nameof(this.Index));
        }

        private bool AccountExists(Guid id)
        {
            return this._context.Users.Any(account => account.Id == id);
        }
    }
}
