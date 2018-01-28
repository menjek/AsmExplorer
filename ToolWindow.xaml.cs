using System.Windows.Controls;

namespace VSAsm
{
    public partial class ToolWindowControl : UserControl
    {
        private const int CompileRow = 1;

        private ToolWindow m_window = null;

        public ToolWindowControl(ToolWindow window)
        {
            m_window = window;
            this.InitializeComponent();
        }
    }
}