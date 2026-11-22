using System.Collections.Generic;
using System.Linq;

namespace HelpTab;

public static class List_Extensions
{
    extension<T>(List<T> list)
    {
        public void AddUnique(T item)
        {
            if (list == null || list.Contains(item))
            {
                return;
            }

            list.Add(item);
        }

        public void AddRangeUnique(IEnumerable<T> items)
        {
            if (list == null || items == null)
            {
                return;
            }

            foreach (var item in items.Where(item => !list.Contains(item)))
            {
                list.Add(item);
            }
        }
    }
}