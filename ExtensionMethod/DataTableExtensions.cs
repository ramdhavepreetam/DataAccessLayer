using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataAccessLayer.ExtensionMethod
{
    public static class DataTableExtensions
    {
        public static IEnumerable<DataRow> AsEnumerable(this DataTable table)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                yield return table.Rows[i];
            }
        }
    }
}
