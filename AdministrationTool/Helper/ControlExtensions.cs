using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdministrationTool.Helper
{
    public static class ControlExtensions
    {
        public static void InvokeIfRequired(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke((MethodInvoker)(() => RunAction(control, action)));
            }
            else
            {
                RunAction(control, action);
            }
        }
        private static void RunAction(Control control, Action action)
        {
            if (control.IsDisposed == false && control.Disposing == false)
                action();
        }
    }
}
