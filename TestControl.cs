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
			this.hScrollBar.Left = 0;
			this.hScrollBar.Top = this.Bottom - this.hScrollBar.Height;
			this.hScrollBar.Width = this.Width;
			this.hScrollBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.Controls.Add(this.hScrollBar);
			this.hScrollBar.ValueChanged += (sender, e) => this.Invalidate();
		}

		private readonly HScrollBar hScrollBar = new HScrollBar();

		public Column[] Columns { get; } = new Column[]
		{
			new Column(100, 40),
			new Column(300, 0),
			new Column(200, 30),
			new Column(0, 30)
		};

		private int GetHScrollValue()
		{
			return this.hScrollBar.Enabled ? this.hScrollBar.Value : 0;
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			this.RecalculateScrollBar();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (this.resizeDragInfo.Active)
			{
				this.Cursor = Cursors.VSplit;
				ColumnPositioning.ResizeDrag(this.resizeDragInfo, e.X, 10);
				this.RecalculateScrollBar();
				return;
			}
			int[] columnWidths = ColumnPositioning.CalculateColumnWidths(this.Columns, this.Width, 10, out _, out _);
			ColumnPositioning.HitTest(columnWidths, this.GetHScrollValue(), e.X, 5, out _, out bool resizeHandle);
			this.Cursor = resizeHandle ? Cursors.VSplit : Cursors.Default;
		}

		private ResizeDragInfo resizeDragInfo;

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button != MouseButtons.Left) return;
			int[] columnWidths = ColumnPositioning.CalculateColumnWidths(this.Columns, this.Width, 10, out _, out _);
			ColumnPositioning.HitTest(columnWidths, this.GetHScrollValue(), e.X, 5, out int columnIndex, out bool resizeHandle);
			if (resizeHandle)
			{
				this.resizeDragInfo = new ResizeDragInfo(this.Columns[columnIndex], e.X);
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (e.Button == MouseButtons.Right && this.resizeDragInfo.Active)
			{
				// Right click during resize - revert
				this.resizeDragInfo.Column.Width = this.resizeDragInfo.StartWidth;
				this.resizeDragInfo = default(ResizeDragInfo);
				this.RecalculateScrollBar();
				return;
			}
			if (e.Button != MouseButtons.Left) return;
			this.resizeDragInfo = default(ResizeDragInfo);
		}

		public void RecalculateScrollBar()
		{
			int availableWidth = this.Width;
			ColumnPositioning.CalculateColumnTotals(this.Columns, 10, out int totalOccupiedWidth, out _);
			int overhang = totalOccupiedWidth - availableWidth;
			int smallChange = this.Width / 20;
			int largeChange = (int)(this.Width / 2.5);
			ScrollBarHelper.SetScrollBarRangeEtc(this.hScrollBar, overhang, smallChange, largeChange);
			this.Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			var g = e.Graphics;
			g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
			g.DrawRectangle(Pens.MediumVioletRed, 0.5f, 0.5f, this.Width - 1, this.Height - 1);

			int totalAvailableWidth = this.Width;
			int[] columnWidths = ColumnPositioning.CalculateColumnWidths(this.Columns, totalAvailableWidth, 10, out int totalOccupiedWidth, out int totalFillableWidth);
			Debug.Assert(columnWidths.Length == this.Columns.Length);

			const int columnHeight = 20;

			int scrollPosition = this.GetHScrollValue();

			int x = -scrollPosition;
			for (int i = 0; i < this.Columns.Length; ++i)
			{
				// Column out of bounds (right). Don't need to draw any more columns.
				if (x > totalAvailableWidth) break;
				var col = this.Columns[i];
				var columnWidth = columnWidths[i];
				if (x + columnWidth < 0)
				{
					// Column out of bounds (left). Go to next column until we find one to draw.
					x += columnWidth;
					continue;
				}

				double effectiveFillPercentage = (double)(columnWidth - col.Width) / totalFillableWidth * 100.0;

				Brush fillBrush = Brushes.Gray;
				g.FillRectangle(fillBrush, x, 0, columnWidth, columnHeight);
				int borderWidth = i == this.Columns.Length - 1 ? columnWidth - 1 : columnWidth;
				g.DrawRectangle(Pens.Black, x + 0.5f, 0.5f, borderWidth, columnHeight - 1.0f);
				TextRenderer.DrawText(g, FormattableString.Invariant($"{col.Width}/{col.FillWeight} = {columnWidth} | {effectiveFillPercentage:F1} %"), this.Font, new Point(x, 0), Color.White);
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
			TextRenderer.DrawText(g, sb.ToString(), this.Font, new Point(0, columnHeight), Color.Black);
		}
	}

	class Column
	{
		public Column(int Width, double FillWeight)
		{
			this.Width = Width;
			this.FillWeight = FillWeight;
		}

		public int Width { get; set; }
		public double FillWeight { get; }
	}
}
