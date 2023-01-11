using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChristMusic.Models.ViewModels
{
    //This View Model combine the pagingInfo and category Model
    public class CategoryVM
    {
        public IEnumerable<Category> Categories { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
