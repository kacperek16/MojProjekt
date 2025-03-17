using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MojProjekt.Data;
using MojProjekt.Models;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MojProjekt.Controllers
{
    public class ItemsController : Controller
    {
        private readonly MyAppContext _context;

        public ItemsController(MyAppContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var item = await _context.Items.Include(c => c.Category)
                                           .ToListAsync();
            return View(item);
        }

        public IActionResult Create()
        {
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Name, Price, CategoryId")] Item item)
        {
            if (ModelState.IsValid)
            {
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(item);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            var item = await _context.Items.FirstOrDefaultAsync(x => x.Id == id);
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id, Name, Price, CategoryId")] Item item)
        {
            if (ModelState.IsValid)
            {
                _context.Update(item);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(item);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Items.FirstOrDefaultAsync(x => x.Id == id);
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                var serialNumbers = _context.SerialNumbers.Where(sn => sn.ItemId == id);
                _context.SerialNumbers.RemoveRange(serialNumbers);
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
        public IActionResult Chart()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetItemsByCategory()
        {
            var itemsByCategory = await _context.Items
                .GroupBy(i => i.Category.Name)
                .Select(g => new
                {
                    category = g.Key,
                    count = g.Count()
                })
                .ToListAsync();

            return Json(itemsByCategory);
        }


        public IActionResult GenerateProducts()
        {
            var products = new List<Item>
            {
                new Item { Name = "Product 1", Price = 10.0, CategoryId = 1 },
                new Item { Name = "Product 2", Price = 20.0, CategoryId = 2 },
                new Item { Name = "Product 3", Price = 30.0, CategoryId = 1 },
                new Item { Name = "Product 4", Price = 40.0, CategoryId = 1 },
                new Item { Name = "Product 5", Price = 50.0, CategoryId = 2 },
                new Item { Name = "Product 6", Price = 60.0, CategoryId = 2 },
                new Item { Name = "Product 7", Price = 70.0, CategoryId = 2 },
                new Item { Name = "Product 8", Price = 80.0, CategoryId = 1 },
                new Item { Name = "Product 9", Price = 90.0, CategoryId = 1 },
                new Item { Name = "Product 10", Price = 100.0, CategoryId = 1 }
            };

            _context.Items.AddRange(products);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult ExportToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var products = _context.Items.Include(c => c.Category).ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Products");
                worksheet.Cells[1, 1].Value = "Name";
                worksheet.Cells[1, 2].Value = "Price";
                worksheet.Cells[1, 3].Value = "Category";

                for (int i = 0; i < products.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = products[i].Name;
                    worksheet.Cells[i + 2, 2].Value = products[i].Price;
                    worksheet.Cells[i + 2, 3].Value = products[i].Category?.Name;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                var content = stream.ToArray();

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products.xlsx");
            }
        }

    }
}

