using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CloudNativeShop.Backend.Models;
using backend.Data;

namespace backend.Controllers
{
    public class ProductController : Controller
    {
        private readonly Product _context;

        public ProductController(Product context)
        {
            _context = context;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            return Ok(await _context.ProductViewModel.Take(10).ToListAsync());
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productViewModel = await _context.ProductViewModel
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (productViewModel == null)
            {
                return NotFound();
            }

            return View(productViewModel);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductCategoryName,ProductNameLength,ProductDescriptionLength,ProductPhotosQty,ProductWeightG,ProductLengthCm,ProductHeightCm,ProductWidthCm")] ProductViewModel productViewModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productViewModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(productViewModel);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productViewModel = await _context.ProductViewModel.FindAsync(id);
            if (productViewModel == null)
            {
                return NotFound();
            }
            return View(productViewModel);
        }

        // POST: Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ProductId,ProductCategoryName,ProductNameLength,ProductDescriptionLength,ProductPhotosQty,ProductWeightG,ProductLengthCm,ProductHeightCm,ProductWidthCm")] ProductViewModel productViewModel)
        {
            if (id != productViewModel.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productViewModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductViewModelExists(productViewModel.ProductId))
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
            return View(productViewModel);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productViewModel = await _context.ProductViewModel
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (productViewModel == null)
            {
                return NotFound();
            }

            return View(productViewModel);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var productViewModel = await _context.ProductViewModel.FindAsync(id);
            if (productViewModel != null)
            {
                _context.ProductViewModel.Remove(productViewModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductViewModelExists(string id)
        {
            return _context.ProductViewModel.Any(e => e.ProductId == id);
        }
    }
}
