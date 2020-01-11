using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColumnWidthTest
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			this.InitializeComponent();
			this.control = new TestControl();
			this.control.Width = this.ClientSize.Width - 150;
			this.control.Height = this.ClientSize.Height;
			this.control.Left = 150;
			this.control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
			this.Controls.Add(this.control);
		}

		private readonly TestControl control;

		private int dragColumn;
		private int dragStartWidth;
		private int dragStartMouseX;
		private bool dragging;

		private void Button1_MouseDown(object sender, MouseEventArgs e)
		{
			int index = (int)this.numericUpDownColumn.Value;
			var col = this.control.Columns[index];
			this.dragColumn = index;
			this.dragStartWidth = col.Width;
			this.dragStartMouseX = e.X;
			this.dragging = true;
		}

		private void Button1_MouseMove(object sender, MouseEventArgs e)
		{
			if (!this.dragging) return;
			var col = this.control.Columns[this.dragColumn];
			int dx = e.X - this.dragStartMouseX;
			col.Width = Math.Max(0, this.dragStartWidth + dx);
			this.control.Invalidate();
		}

		private void Button1_MouseUp(object sender, MouseEventArgs e)
		{
			this.dragging = false;
		}
	}
}
