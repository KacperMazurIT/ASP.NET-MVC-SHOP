﻿using shop.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace shop.Models.ViewModels.Pages
{
    public class PageVM
    {
        public PageVM()
        {

        }

        public PageVM(PageDTO row)
        {
            Id = row.Id;
            Title = row.Title;
            Slug = row.Slug;
            Body = row.Body;
            Sorting = row.Sorting;
            HasSidebar = row.HasSidebar;

        }

        public int Id { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [Display(Name ="Tytuł Strony")]
        public string Title { get; set; }
        [Display(Name = "Adres Strony")]
        public string Slug { get; set; }
        [Required]
        [StringLength(int.MaxValue, MinimumLength = 3)]
        [Display(Name = "Zawartość Strony")]
        [AllowHtml]
        public string Body { get; set; }
        public int Sorting { get; set; }
        [Display(Name = "Pasek Boczny")]
        public bool HasSidebar { get; set; }
    }
}