﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using X.PagedList;
using shop.Models.Data;
using shop.Models.ViewModels.Shop;


namespace shop.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {

            // deklaracja listy kategorii do wyswietlenia
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                categoryVMList = db.Categories
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoryVM(x)).ToList();
            }

            return View(categoryVMList);
        }

        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            // Deklaracja id
            string id;

            using (Db db = new Db())
            {
                // sprwdzenie czy nazwa kategorii jest unikalna
                if(db.Categories.Any(x => x.Name == catName))
                    return "tytulzajety";

                // inicjalizacja DTO
                CategoryDTO dto = new CategoryDTO();
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 1000;

                // zapis do bazy
                db.Categories.Add(dto);
                db.SaveChanges();

                // pobieramy id
                id = dto.Id.ToString();

            }

            return id;
        }

        //POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public ActionResult ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                // inicjalizacja licznika
                int count = 1;

                // deklaracja DTO
                CategoryDTO dto;

                // sortowanie kategorii
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    // zapis na bazie
                    db.SaveChanges();

                    count++;
                }
            }

            return View();
        }

        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            using(Db db = new Db())
            {
                // download category name
                CategoryDTO dto = db.Categories.Find(id);

                // usuwamy kategorie
                db.Categories.Remove(dto);

                // zapis na bazie
                db.SaveChanges();

            }

            return RedirectToAction("Categories");
        }

        // POST: Admin/Shop/RenameCategory
        [HttpPost]
        public string  RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                // Sprawdzenie czy kategoria jest unikalna
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "tytulzajety";

                // pobieramy kategorię
                CategoryDTO dto = db.Categories.Find(id);

                // edycja kategorii

                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                // zapis na bazie
                db.SaveChanges();
            }

            return "Ok";
        }

        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            // Inicjalizacja modelu

            ProductVM model = new ProductVM();

            // pobieramy liste kategorii 
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            return View(model);
        }

        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            // Sprawdzamy model state
            if(!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            // sprawdzenie czy nazwa produktu jest unikalna

            using (Db db = new Db())
            {
               if (db.Products.Any(x => x.Name == model.Name))
               {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("","Ta nazwa produktu jest zajęta!");
                    return View(model);
               }
            }

            // deklaracja produkt id
            int id;

            // dodanie produktu i zapis na bazie

            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);

                product.CategoryName = catDTO.Name;

                db.Products.Add(product);

                db.SaveChanges();

                // pobranie id dodanego produktu
                id = product.Id;

            }

            //ustawiamy komunikat
            TempData["SM"] = "Dodałeś produkt";

            #region Upload Image

            // Utworzenie potrzebnej struktury katalogów 
            var orginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString1 = Path.Combine(orginalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() );
            var pathString3 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs" );
            var pathString4 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery" );
            var pathString5 = Path.Combine(orginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs" );

            if (!Directory.Exists(pathString1))
            {
                Directory.CreateDirectory(pathString1);
            }

            if (!Directory.Exists(pathString2))
            {
                Directory.CreateDirectory(pathString2);
            }

            if (!Directory.Exists(pathString3))
            {
                Directory.CreateDirectory(pathString3);
            }

            if (!Directory.Exists(pathString4))
            {
                Directory.CreateDirectory(pathString4);
            }

            if (!Directory.Exists(pathString5))
            {
                Directory.CreateDirectory(pathString5);
            }

            if(file != null && file.ContentLength > 0)
            {
                // sprawdzenie rozszerzenia pliku czy mamy doczynienia z obrazkiem
                string ext = file.ContentType.ToLower();
                if (ext != "image/jpg" &&  
                    ext != "image/jpeg" && 
                    ext != "image/pjpeg"  && 
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png"
                    )
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Obraz nie został przesłany - nieprawidłowe rozszerzenie obrazu.");
                        return View(model);
                    }           
                }

                // inicjalizacja nazwy obrazka
                string imageName = file.FileName;

                // zapis nazwy obrazka do bazy 
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                var path1 = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                // zapisujemy orginalny obrazek
                file.SaveAs(path1);

                // zapisujemy miniaturkę
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);

            }

            #endregion Upload Image

            return RedirectToAction("AddProduct");
        }

        // GET: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            // Deklaracja listy Produktów
            List<ProductVM> listOfProductVM;

            // Ustawiamy numer strony
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                // inicjalizacja listy produktów
                listOfProductVM = db.Products.ToArray()
                                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                    .Select(x => new ProductVM(x))
                                    .ToList();

                // lista kategori do dropDownList
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // ustawiamy wybrana ketegorię
                ViewBag.SelectedCat = catId.ToString();
            }

            // ustawienie stronnicowanie
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            // zwracamy widok z lista produktow
            return View(listOfProductVM);
        }

        // GET: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            // deklaracja productVM
            ProductVM model;

            using (Db db = new Db())
            {
                // pobieramy produkt do edycji
                ProductDTO dto =  db.Products.Find(id);

                // sprawdzenie czy produkt istnieje
                if (dto == null)
                {
                    return Content("Ten produkt nie istnieje");
                }

                // inicjalizacja modelu
                model = new ProductVM(dto);

                // lista kategorii
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // ustawiamy zdjecia
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }

            return View(model);
        }

        // POST: Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            // pobranie id produktu
            int id = model.Id;

            // pobranie kategorii dla listy rozwijanej
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            // ustawiamy zdjecia
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                            .Select(fn => Path.GetFileName(fn));

            // sprawdzamy model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // sprawdzenie unikalnosci nazwy produktu
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "Ta nazwa produktu jest zajęta");
                    return View(model);
                }
            }

            // Edycja produktu i zapis na bazie
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDto.Name;

                db.SaveChanges();
            }

            // ustawienie TempData
            TempData["SM"] = "Edytowałeś produkt";

            #region Image Upload

            // sprawdzamy czy jest plik
            if (file != null && file.ContentLength > 0)
            {
                // sprawdzamy rozszerzenie pliku czy to jest obrazek
                string ext = file.ContentType.ToLower();

                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Obraz nie został przesłany - nieprawidłowe rozszerzenie obrazu.");
                        return View(model);
                    }
                }

                // Utworzenie potrzebnej struktury katalogów
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                // usuwamy stare pliki z katalogow
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file2 in di1.GetFiles())
                    file2.Delete();

                foreach (var file3 in di2.GetFiles())
                    file3.Delete();

                // Zapis nazwy obrazka na bazie
                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                // Zapis nowych plików
                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            return RedirectToAction("EditProduct");
        }

        // GET: Admin/Shop/DeleteProduct/id
        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            // Usunięcie produktu z bazy
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }

            // usunięcie folderu produktu z wszystkimi plikami
            var orginalDirector = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString = Path.Combine(orginalDirector.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
            {
                Directory.Delete(pathString, true);
            }

            return RedirectToAction("Products");
        }

    }
}