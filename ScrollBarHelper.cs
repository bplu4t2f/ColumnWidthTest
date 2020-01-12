using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColumnWidthTest
{
	static class ScrollBarHelper
	{
		public static void SetScrollBarRangeEtc(ScrollBar scrollBar, int max, int smallChange, int largeChange)
		{
			if (max <= 0)
			{
				scrollBar.Enabled = false;
				return;
			}
			if (smallChange <= 1) smallChange = 1;
			if (largeChange < smallChange) largeChange = smallChange;
			scrollBar.Enabled = true;
			scrollBar.SmallChange = 0;
			scrollBar.LargeChange = 0;
			scrollBar.Maximum = max + largeChange - 1;
			scrollBar.SmallChange = smallChange;
			scrollBar.LargeChange = largeChange;
			scrollBar.Value = scrollBar.Value = Math.Min(max, scrollBar.Value);
		}
	}
}
