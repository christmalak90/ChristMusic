using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChristMusic.Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }

        //Contains the List of Categories to be populated in a dropdownlist in the view
        public IEnumerable<SelectListItem> CategoryList { get; set; }

        //Contains the List of CoverTypes to be populated in a dropdownlist in the view
        public IEnumerable<SelectListItem> CoverTypeList { get; set; }

    }
}
