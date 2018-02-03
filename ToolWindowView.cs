using System.Windows.Controls;

namespace VSAsm
{
    class ToolWindowView
    {
        #region Constants.

        const int TextBoxMinWidth = 1024;

        #endregion // Constants.

        #region Data.

        RichTextBox m_textBox;

        #endregion // Data.

        #region Create.

        public ToolWindowView(RichTextBox textBox)
        {
            m_textBox = textBox;
            m_textBox.Document.MinPageWidth = TextBoxMinWidth;
        }

        #endregion // Create.

        #region Interface.

        public void Display(AsmFunction function)
        {
        }

        public void Display(AsmFile file)
        {
        }

        #endregion // Interface.
    }
}
