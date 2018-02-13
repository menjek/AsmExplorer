using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.VCProjectEngine;
using System;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace VSAsm
{
    class ToolWindowView
    {
        #region Constants.

        const int TextBoxMinWidth = 1024;
        static readonly Guid TextEditorFontGuid = new Guid(FontsAndColorsCategory.TextEditor);

        #endregion // Constants.

        #region Data.

        ToolWindow m_window = null;
        RichTextBox m_textBox = null;
        double m_zoomLevel = 1.0;
        double m_fontSize = 0.0;

        #endregion // Data.

        #region Create.

        public ToolWindowView(ToolWindow window, RichTextBox textBox)
        {
            m_window = window;
            m_textBox = textBox;
            m_textBox.Document.MinPageWidth = TextBoxMinWidth;

            RegisterForTextManagerEvents();
            UpdateFont();

            TextViewCreationListener.Events += (IWpfTextView textView) => textView.ZoomLevelChanged += UpdateZoom;
        }

        #endregion // Create.

        #region Interface.

        public void OnDocumentChanged()
        {
            if (m_window.ActiveFile == null) {
                SetupNoDocument();
            } else if (m_window.ActiveAsm == null) {
                SetupMissingAsm();
            } else {
                SetupAsm();
            }
        }

        public void OnLineChanged()
        {
            if (m_window.ActiveAsm != null) {
                SetupAsm();
            }
        }

        #endregion // Interface.

        #region States

        void SetupNoDocument()
        {
            m_textBox.Document.Blocks.Clear();
        }

        void SetupMissingAsm()
        {
            m_textBox.Document.Blocks.Clear();
            m_textBox.AppendText("No compiled assembly.");
        }

        class FunctionLineComparer : IComparer
        {
            public int Compare(object lhs, object rhs)
            {
                int lhsLine = 0;
                int rhsLine = 0;

                if (lhs is AsmFunction lhsFunction) {
                    lhsLine = lhsFunction.Range.Min;
                    rhsLine = (int)rhs;
                } else {
                    lhsLine = (int)lhs;
                    rhsLine = ((AsmFunction)rhs).Range.Min;
                }

                if (lhsLine < rhsLine) {
                    return -1;
                } else if (rhsLine < lhsLine) {
                    return 1;
                } else {
                    return 0;
                }
            }
        }

        void SetupAsm()
        {
            m_textBox.Document.Blocks.Clear();

            AsmUnit asm = m_window.ActiveAsm;
            AsmFile file = asm.Files[m_window.ActiveFile.FullPath.ToLower()];

            AsmFunction selected = null;

            int functionIndex = Array.BinarySearch(file.Functions, m_window.CurrentLine, new FunctionLineComparer());
            if (functionIndex >= 0) {
                selected = file.Functions[functionIndex];
            } else {
                functionIndex = ~functionIndex - 1;
                if (functionIndex >= 0) {
                    selected = file.Functions[functionIndex];
                }
            }

            if (selected != null && selected.Range.Contains(m_window.CurrentLine)) {
                SetupFunction(selected);
            }
        }

        void SetupFunction(AsmFunction function)
        {
            m_textBox.AppendText(function.Name + Environment.NewLine);
            foreach (AsmBlock block in function.Blocks) {
                string paragraph = string.Join(Environment.NewLine, block.Assembly);
                m_textBox.Document.Blocks.Add(new Paragraph(new Run(paragraph)));
            }
        }

        #endregion

        #region Events.

        class TextManagerEventHandler : IVsTextManagerEvents
        {
            public ToolWindowView View { get; set; }

            public void OnRegisterMarkerType(int markerType) { }
            public void OnRegisterView(IVsTextView view) { }
            public void OnUnregisterView(IVsTextView view) { }

            public void OnUserPreferencesChanged(VIEWPREFERENCES[] viewPrefs,
                FRAMEPREFERENCES[] framePrefs,
                LANGPREFERENCES[] langPrefs,
                FONTCOLORPREFERENCES[] colorPrefs)
            {
                View.UpdateFont();
            }
        }

        void RegisterForTextManagerEvents()
        {
            IConnectionPointContainer container = (IConnectionPointContainer)Package.GetGlobalService(typeof(SVsTextManager));
            if (container == null) {
                return;
            }

            Guid eventsGuid = typeof(IVsTextManagerEvents).GUID;
            container.FindConnectionPoint(ref eventsGuid, out IConnectionPoint textManagerEventsConnection);

            TextManagerEventHandler handler = new TextManagerEventHandler() {
                View = this
            };

            textManagerEventsConnection.Advise(handler, out uint textManagerCookie);
        }

        void UpdateFont()
        {
            FontInfo? info = GetTextEditorFontInfo();
            if (info.HasValue) {
                m_textBox.FontFamily = new FontFamily(info.Value.bstrFaceName);
                m_fontSize = PointsToPixels(info.Value.wPointSize);
                m_textBox.FontSize = m_fontSize * m_zoomLevel;
            }
        }

        FontInfo? GetTextEditorFontInfo()
        {
            IVsFontAndColorStorage fontStorage = (IVsFontAndColorStorage)Package.GetGlobalService(typeof(SVsFontAndColorStorage));
            if (fontStorage == null) {
                return null;
            }

            if (fontStorage.OpenCategory(TextEditorFontGuid, (uint)(__FCSTORAGEFLAGS.FCSF_LOADDEFAULTS)) != VSConstants.S_OK) {
                return null;
            }

            FontInfo[] info = new FontInfo[1];
            int result = fontStorage.GetFont(null, info);
            fontStorage.CloseCategory();

            if (result != VSConstants.S_OK) {
                return null;
            }

            return info[0];
        }

        static double PointsToPixels(int points)
        {
            return (points * 96.0) / 72.0;
        }

        void UpdateZoom(object sender, ZoomLevelChangedEventArgs args)
        {
            m_zoomLevel = args.NewZoomLevel / 100.0;
            m_textBox.FontSize = m_fontSize * m_zoomLevel;
        }

        #endregion // Events.
    }
}
