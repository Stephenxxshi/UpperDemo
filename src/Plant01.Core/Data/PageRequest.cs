using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plant01.Core.Data
{
    public class PageRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchText { get; set; }
        public string SortField { get; set; }
        public bool IsAscending { get; set; }
    }
}
