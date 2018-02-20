using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
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
            if (m_window.ActiveTextView == null) {
                SetupNoDocument();
            } else if (m_window.ActiveFile == null) {
                SetupNoSource();
            } else if (m_window.ActiveAsm == null) {
                SetupNoAsm();
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

        public void OnDirtyStateChanged(bool isDirty)
        {
            // Do not handle at the moment.
            // This may be more complicated when assembly parsing
            // from COFF and PDBs is implemented.
        }

        #endregion // Interface.

        #region States

        void SetupNoDocument()
        {
            m_textBox.Document.Blocks.Clear();
            m_textBox.AppendText("No document.");
        }

        void SetupNoSource()
        {
            m_textBox.Document.Blocks.Clear();
            m_textBox.AppendText("No source.");
        }

        void SetupNoAsm()
        {
            m_textBox.Document.Blocks.Clear();
            m_textBox.AppendText("No asm.");
        }

        void SetupAsm()
        {
            m_textBox.Document.Blocks.Clear();

            AsmFunction function = SearchFunction(m_window.ActiveAsm.Functions, m_window.CurrentLine);
            if (function != null) {
                SetupFunction(function);
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

        static AsmFunction SearchFunction(List<AsmFunction> functions, int line)
        {
            int first = 0;
            int count = functions.Count;

            while (0 < count) {
                int middle = count / 2;
                AsmFunction middleFunction = functions[first + middle];
                if (middleFunction.Range.Min < line) {
                    first = middle + 1;
                    count -= middle + 1;
                } else {
                    count = middle;
                }
            }

            if (first == functions.Count) {
                return null;
            }

            AsmFunction function = functions[first];
            if (function.Range.Contains(line)) {
                return function;
            }

            if (first != 0) {
                function = functions[first - 1];
                if (function.Range.Contains(line)) {
                    return function;
                }
            }

            return null;
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
