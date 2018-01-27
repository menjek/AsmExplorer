using System.Windows.Controls;

namespace VSAsm
{
    public partial class EditorWindowControl : UserControl
    {
        private const int CompileRow = 1;

        private EditorWindow m_window = null;

        public EditorWindowControl(EditorWindow window)
        {
            m_window = window;
            this.InitializeComponent();
        }
    }
}