using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Windows.Controls;
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

        RichTextBox m_textBox = null;
        double m_zoomLevel = 1.0;
        double m_fontSize = 0.0;

        #endregion // Data.

        #region Create.

        public ToolWindowView(RichTextBox textBox)
        {
            m_textBox = textBox;
            m_textBox.Document.MinPageWidth = TextBoxMinWidth;

            RegisterForTextManagerEvents();
            UpdateFont();

            TextViewCreationListener.Events += (IWpfTextView textView) => textView.ZoomLevelChanged += UpdateZoom;
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
