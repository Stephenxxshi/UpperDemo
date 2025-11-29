using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plant01.Core.Data
{
    public class PagedResult<T> : DataResult<IEnumerable<T>>
    {
        public int TotalCount { get; set; }

        public static PagedResult<T> Success(IEnumerable<T> items, int totalCount)
        {
            return new PagedResult<T>
            {
                IsSuccess = true,
                Content = items,
                TotalCount = totalCount
            };
        }
    }
}
