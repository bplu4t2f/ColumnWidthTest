using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColumnWidthTest
{
	class TestControl : Control
	{
		public TestControl()
		{
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
		}

		public Column[] Columns { get; } = new Column[]
		{
			new Column(100, 40, 150),
			new Column(300, 0, 100),
			new Column(200, 30, 100),
			new Column(0, 30, 110)
		};

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			var g = e.Graphics;
			g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
			g.DrawRectangle(Pens.MediumVioletRed, 0.5f, 0.5f, this.Width - 1, this.Height - 1);

			int totalAvailableWidth = this.Width;
			int[] columnWidths = ColumnPositioning.CalculateColumnWidths(this.Columns, totalAvailableWidth, out int totalOccupiedWidth, out int totalFillableWidth);
			Debug.Assert(columnWidths.Length == this.Columns.Length);

			const int columnHeight = 20;

			int x = 0;
			for (int i = 0; i < this.Columns.Length; ++i)
			{
				var col = this.Columns[i];
				var columnWidth = columnWidths[i];

				bool isLimitedByMinSize = columnWidth <= col.MinWidth;
				double effectiveFillPercentage = (double)(columnWidth - col.Width) / totalFillableWidth * 100.0;

				Brush fillBrush = isLimitedByMinSize ? Brushes.IndianRed : Brushes.Gray;
				g.FillRectangle(fillBrush, x, 0, columnWidth, columnHeight);
				g.DrawRectangle(Pens.Black, x, 0, columnWidth, columnHeight);
				g.DrawString(FormattableString.Invariant($"{col.Width}/{col.FillWeight}/{col.MinWidth} = {columnWidth} | {effectiveFillPercentage:F1} %"), this.Font, Brushes.White, x, 0);
				x += columnWidth;
			}

			var sb = new StringBuilder();
			foreach (var width in columnWidths)
			{
				sb.AppendLine(width.ToString());
			}
			sb.AppendLine($"Total available: {totalAvailableWidth}");
			sb.AppendLine($"Total fillable: {totalFillableWidth}");
			sb.AppendLine($"Total occupied: {totalOccupiedWidth}");
			g.DrawString(sb.ToString(), this.Font, Brushes.Black, 0, columnHeight);
		}
	}

	class Column
	{
		public Column(int Width, double FillWeight, int MinWidth)
		{
			this.Width = Width;
			this.FillWeight = FillWeight;
			this.MinWidth = MinWidth;
		}

		public int Width { get; set; }
		public double FillWeight { get; }
		public int MinWidth { get; }
	}
}
