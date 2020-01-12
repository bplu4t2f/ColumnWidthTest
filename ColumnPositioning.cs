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
		public static void CalculateColumnTotals(IList<Column> columns, int minimumColumnWidth, out int occupiedByStraightWidth, out double totalWeight)
		{
			if (columns == null) throw new ArgumentNullException(nameof(columns));

			occupiedByStraightWidth = 0;
			totalWeight = 0;
			for (int i = 0; i < columns.Count; ++i)
			{
				var col = columns[i];
				occupiedByStraightWidth += Math.Max(minimumColumnWidth, col.Width);
				totalWeight += Math.Max(0.0, col.FillWeight);
			}
		}

		public static int[] CalculateColumnWidths(IList<Column> columns, int totalAvailableWidth, int minimumColumnWidth, out int totalOccupiedWidth, out int totalFillableWidth)
		{
			CalculateColumnTotals(columns, minimumColumnWidth, out int occupiedByStraightWidth, out double totalWeight);

			totalFillableWidth = Math.Max(0, totalAvailableWidth - occupiedByStraightWidth);

			var actualColumnWidths = new int[columns.Count];
			totalOccupiedWidth = 0;
			for (int i = 0; i < columns.Count; ++i)
			{
				var col = columns[i];
				double clampedFillWeight = Math.Max(0.0, col.FillWeight);
				double filledPart = totalWeight > 0.0 ? totalFillableWidth * clampedFillWeight / totalWeight : 0.0;
				int width = Math.Max(minimumColumnWidth, col.Width) + (int)filledPart;
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

		public static void ResizeDrag(ResizeDragInfo dragInfo, int mouseX, int minimumColumnWidth)
		{
			if (!dragInfo.Active) return;
			int dx = mouseX - dragInfo.MouseX;
			dragInfo.Column.Width = Math.Max(minimumColumnWidth, dragInfo.StartWidth + dx);
		}

		public static void HitTest(int[] columnWidths, int scrollValue, int mouseX, int resizeHandleDistance, out int columnIndex, out bool resizeHandle)
		{
			int x = 0;
			for (int i = 0; i < columnWidths.Length; ++i)
			{
				int width = columnWidths[i];
				int startPixel = x - scrollValue;
				int endPixel = startPixel + width;
				int resizeHandleStart = endPixel - resizeHandleDistance;
				int resizeHandleEnd = endPixel + resizeHandleDistance;
				if (mouseX >= resizeHandleStart && mouseX <= resizeHandleEnd)
				{
					resizeHandle = true;
					columnIndex = i;
					return;
				}
				if (mouseX >= startPixel && mouseX < endPixel)
				{
					resizeHandle = false;
					columnIndex = i;
					return;
				}
				x += width;
			}
			columnIndex = -1;
			resizeHandle = default(bool);
		}
	}

	struct ResizeDragInfo
	{
		public ResizeDragInfo(Column column, int mouseX)
		{
			this.Active = true;
			this.Column = column ?? throw new ArgumentNullException(nameof(column));
			this.StartWidth = column.Width;
			this.MouseX = mouseX;
		}

		public bool Active { get; }
		public Column Column { get; }
		public int StartWidth { get; }
		public int MouseX { get; }
	}
}
