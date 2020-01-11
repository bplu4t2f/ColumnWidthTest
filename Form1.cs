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
			var control = new TestControl();
			control.Size = this.ClientSize;
			control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
			this.Controls.Add(control);
		}
	}
}
