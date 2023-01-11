using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChristMusic.Models
{
    public class PagingInfo
    {
        public int TotalItem { get; set; }
        public int ItemPerPage { get; set; }
        public int CurrentPage { get; set; }
        public string urlParam { get; set; }

        public int TotalPage => (int)Math.Ceiling((decimal)TotalItem/ItemPerPage);
    }
}
