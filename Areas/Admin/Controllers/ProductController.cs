using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Runtime.CompilerServices;

namespace Bulky_Book.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: ProductController
        public IActionResult Index()
        {
            List<Product> objProductList = _productRepository.GetAll(includeProperties: "Category").ToList();
            return View(objProductList);
        }

        // GET: ProductController/Details/5
        public IActionResult Details(int id)
        {
            return View();
        }

        // GET: ProductController/Create
        public ActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new Product(),
                CategoryList = _categoryRepository.GetAll()
                .Select(item => new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                })
            };

            if (id != null && id != 0)
            {
                productVM.Product = _productRepository.GetFirstOrDefault(item => item.Id == id);
            }

            return View(productVM);
        }

        // POST: ProductController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                productVM.CategoryList = _categoryRepository.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
            }

            string wwwRootPath = _webHostEnvironment.WebRootPath;

            if (file != null && !string.IsNullOrEmpty(productVM.Product.ImageUrl))
            {
                string oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images\Product");
                using var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create);
                file.CopyTo(fileStream);

                productVM.Product.ImageUrl = @"\images\product\" + fileName;
            }

            if (productVM.Product.Id == 0)
            {
                _productRepository.Add(productVM.Product);
            } else
            {
                _productRepository.Update(productVM.Product);
            }

            _productRepository.Save();
            TempData["success"] = "Product was set successfully";
            return RedirectToAction("Index");
        }

        // GET: ProductController/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Product? retrievedCategory = _productRepository.GetFirstOrDefault(item => item.Id == id);
            if (retrievedCategory == null)
            {
                return NotFound();
            }
            return View(retrievedCategory);
        }

        // POST: ProductController/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            Product? obj = _productRepository.GetFirstOrDefault(item => item.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _productRepository.Remove(obj);
            _productRepository.Save();
            TempData["success"] = "Product was deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
