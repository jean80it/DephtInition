using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace System.Windows.Forms
{
    public class DblBufPanel : Panel
    {
        public DblBufPanel()
            :base()
        {
            this.DoubleBuffered = true;
        }
    }
}
