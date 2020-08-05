using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
    }
}