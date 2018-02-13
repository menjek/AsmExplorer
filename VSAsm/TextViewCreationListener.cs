using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace VSAsm
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    class TextViewCreationListener : IWpfTextViewCreationListener
    {
        public delegate void TextViewCreatedHandler(IWpfTextView textView);
        public static event TextViewCreatedHandler Events;

        public void TextViewCreated(IWpfTextView textView)
        {
            Events(textView);
        }
    }
}
