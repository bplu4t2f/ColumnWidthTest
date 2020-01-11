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
		public static int[] CalculateColumnWidths(IList<Column> columns, int totalAvailableWidth, out int totalOccupiedWidth, out int totalFillableWidth)
		{
			if (columns == null) throw new ArgumentNullException(nameof(columns));

			totalFillableWidth = totalAvailableWidth;
			double totalWeight = 0;
			for (int i = 0; i < columns.Count; ++i)
			{
				var col = columns[i];
				totalFillableWidth -= col.Width;
				totalWeight += col.FillWeight;
			}
			totalFillableWidth = Math.Max(0, totalFillableWidth);

			int totalRemainingWidthToFulfillMinimum = 0;
			var remainingWeights = new double[columns.Count];
			double totalRemainingWeight = 0;
			int totalRemainingFillableWidth;
			if (totalFillableWidth <= 0)
			{
				totalRemainingFillableWidth = 0;
			}
			else
			{
				for (int i = 0; i < columns.Count; ++i)
				{
					var col = columns[i];
					int remainingWidthToFulfillMinimumRequirement = Math.Max(0, col.MinWidth - col.Width);
					totalRemainingWidthToFulfillMinimum += remainingWidthToFulfillMinimumRequirement;
					double partToFulfillMinimum = (double)remainingWidthToFulfillMinimumRequirement / totalFillableWidth;
					double weightToFulfillMinimum = partToFulfillMinimum * totalWeight;
					double remainingWeight = Math.Max(0, col.FillWeight - weightToFulfillMinimum);
					remainingWeights[i] = remainingWeight;
					totalRemainingWeight += remainingWeight;
				}
				totalRemainingFillableWidth = Math.Max(0, totalFillableWidth - totalRemainingWidthToFulfillMinimum);
			}

			var actualColumnWidths = new int[columns.Count];
			totalOccupiedWidth = 0;
			for (int i = 0; i < columns.Count; ++i)
			{
				var col = columns[i];
				int remainingWidthToFulfillMinimumRequirement = Math.Max(0, col.MinWidth - col.Width);
				int fillingWidth = totalRemainingWeight <= 0 ? 0 : Math.Max(0, (int)(remainingWeights[i] / totalRemainingWeight * totalRemainingFillableWidth));
				int width = col.Width + remainingWidthToFulfillMinimumRequirement + fillingWidth;
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

		public static void ResizeDrag(ResizeDragInfo dragInfo, int mouseX)
		{
			if (!dragInfo.Active) return;
			int dx = mouseX - dragInfo.MouseX;
			dragInfo.Column.Width = Math.Max(0, dragInfo.StartWidth + dx);
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
