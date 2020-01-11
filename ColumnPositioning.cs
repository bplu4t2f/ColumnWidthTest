using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColumnWidthTest
{
	static class ColumnPositioning
	{
		public static int[] CalculateColumnWidths(IList<Column> columns, int totalAvailableWidth, out int totalOccupiedWidth)
		{
			if (columns == null) throw new ArgumentNullException(nameof(columns));

			int totalFillableWidth = totalAvailableWidth;
			double totalWeight = 0;
			for (int i = 0; i < columns.Count; ++i)
			{
				var col = columns[i];
				totalFillableWidth -= col.Width;
				totalWeight += col.FillWeight;
			}

			var actualColumnWidths = new int[columns.Count];
			totalOccupiedWidth = 0;
			for (int i = 0; i < columns.Count; ++i)
			{
				var col = columns[i];
				int width = col.Width + Math.Max(0, (int)(col.FillWeight / totalWeight * totalFillableWidth));
				actualColumnWidths[i] = width;
				totalOccupiedWidth += width;
			}

			// Because of rasterization, conversion of weights might leave a gap at the end. We append that to the last column.
			int waste = totalAvailableWidth - totalOccupiedWidth;
			if (waste > 0 && columns.Count > 0)
			{
				actualColumnWidths[columns.Count - 1] += waste;
				totalOccupiedWidth += waste;
			}

			return actualColumnWidths;
		}
	}
}
