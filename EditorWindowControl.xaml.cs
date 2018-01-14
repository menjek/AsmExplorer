using System.Windows.Controls;

namespace AsmExplorer
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

        public void ShowCompile()
        {
            this.AsmGrid.RowDefinitions[CompileRow].MaxHeight = double.PositiveInfinity;
        }

        public void HideCompile()
        {
            this.AsmGrid.RowDefinitions[CompileRow].MaxHeight = 0;
        }

        private void AsmCompile_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            m_window.CompileActive();
        }
    }
}