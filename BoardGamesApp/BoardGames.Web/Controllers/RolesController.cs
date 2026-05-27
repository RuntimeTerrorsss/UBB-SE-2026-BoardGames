// <copyright file="RolesController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data;
using BoardGames.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoardGames.Web.Controllers
{
    public class RolesController : Controller
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            this._context = context;
        }

        // GET: Roles
        public async Task<IActionResult> Index()
        {
            return this.View(await this._context.Roles.ToListAsync());
        }

        // GET: Roles/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var role = await this._context.Roles
                .FirstOrDefaultAsync(inputAccount => inputAccount.Id == id);
            if (role == null)
            {
                return this.NotFound();
            }

            return this.View(role);
        }

        // GET: Roles/Create
        public IActionResult Create()
        {
            return this.View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Role role)
        {
            if (this.ModelState.IsValid)
            {
                role.Id = Guid.NewGuid();
                this._context.Add(role);
                await this._context.SaveChangesAsync();
                return this.RedirectToAction(nameof(this.Index));
            }

            return this.View(role);
        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var role = await this._context.Roles.FindAsync(id);
            if (role == null)
            {
                return this.NotFound();
            }

            return this.View(role);
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name")] Role role)
        {
            if (id != role.Id)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                try
                {
                    this._context.Update(role);
                    await this._context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!this.RoleExists(role.Id))
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

            return this.View(role);
        }

        // GET: Roles/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var role = await this._context.Roles
                .FirstOrDefaultAsync(inputRole => inputRole.Id == id);
            if (role == null)
            {
                return this.NotFound();
            }

            return this.View(role);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var role = await this._context.Roles.FindAsync(id);
            if (role != null)
            {
                this._context.Roles.Remove(role);
            }

            await this._context.SaveChangesAsync();
            return this.RedirectToAction(nameof(this.Index));
        }

        private bool RoleExists(Guid id)
        {
            return this._context.Roles.Any(inputRole => inputRole.Id == id);
        }
    }
}
