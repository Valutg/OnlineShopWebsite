using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using OnlineShopExample.Data;
using OnlineShopExample.Models;
using OnlineShopExample.Models.ViewModels;

namespace OnlineShopExample.Controllers
{
    [Authorize(Roles = WC.AdmineRole)]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;   
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> objList = _db.Product;

            foreach (var obj in objList)
            {
                obj.Category = _db.Category.FirstOrDefault(u => u.Id == obj.CategoryId);
            }

            return View(objList);
        }

        // Get - Upsert
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _db.Category.Select(i => new SelectListItem
                {
                    Text = i.CategoryName,
                    Value = i.Id.ToString()
                })
            };

            if (id == null)
            {
                // this is for create
                return View(productVM);
            }
            else
            {
                productVM.Product = _db.Product.Find(id);
                if (productVM.Product  == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }
        }

        // Post - Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            var files = HttpContext.Request.Form.Files;
            string webRootPath = _webHostEnvironment.WebRootPath;

            if (productVM.Product.Id == 0)
            {
                //Creating
                string upload = webRootPath + WC.ImagePath;
                string fileName = Guid.NewGuid().ToString();
                string extenstion = Path.GetExtension(files[0].FileName);

                using (var fileStream = new FileStream(Path.Combine(upload, fileName + extenstion), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }

                productVM.Product.Image = fileName + extenstion;

                _db.Product.Add(productVM.Product);

            }
            else
            {
                //Updating
                var objFromDb = _db.Product.AsNoTracking().FirstOrDefault(p => p.Id == productVM.Product.Id);
                if (files.Count> 0)
                {
                    string upload = webRootPath + WC.ImagePath;
                    string fileName = Guid.NewGuid().ToString();
                    string extenstion = Path.GetExtension(files[0].FileName);

                    var oldFile = Path.Combine(upload, objFromDb.Image);

                    if (System.IO.File.Exists(oldFile))
                    {
                        System.IO.File.Delete(oldFile);
                    }
                    
                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extenstion), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    productVM.Product.Image = fileName + extenstion;

                }
                else
                {
                    productVM.Product.Image = objFromDb.Image;
                }
                _db.Product.Update(productVM.Product);
            }
            _db.SaveChanges();
            return RedirectToAction("Index");
        }



        // Get - Delete
        public IActionResult Delete(int? id)
        {
            if (id == null && id == 0)
            {
                return NotFound();
            }
            
            Product product = _db.Product.Include(u => u.Category).FirstOrDefault(u => u.Id == id) ;

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // Post - Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.Product.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            string upload = _webHostEnvironment.WebRootPath + WC.ImagePath;


            var oldFile = Path.Combine(upload, obj.Image);

            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }
            _db.Product.Remove(obj);
            _db.SaveChanges();
            return RedirectToAction("Index");

        }


    }
}
