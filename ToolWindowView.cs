using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Windows.Controls;
using System.Windows.Documents;

namespace VSAsm
{
    class ToolWindowView : IVsTextManagerEvents
    {
        #region Constants.

        const int TextBoxMinWidth = 1024;

        #endregion // Constants.

        #region Data.

        RichTextBox m_textBox;
        double m_zoomLevel = 100;

        #endregion // Data.

        #region Create.

        public ToolWindowView(RichTextBox textBox)
        {
            m_textBox = textBox;
            m_textBox.Document.MinPageWidth = TextBoxMinWidth;

            m_textBox.Document.Blocks.Clear();
            m_textBox.Document.Blocks.Add(new Paragraph(new Run("#include <iostream>\n#include <array>\n#include <vector>\n#include <string>\nusing namespace std;")));

            RegisterForTextManagerEvents();
            UpdateFont();

            TextViewCreationListener.Events += (IWpfTextView textView) => textView.ZoomLevelChanged += Zoom;
        }

        #endregion // Create.

        #region Interface.

        public void Display(AsmFunction function)
        {
        }

        public void Display(AsmFile file)
        {
        }

        public void UpdateFont()
        {
            var pLOGFONT = new LOGFONTW[1];
            var pInfo = new FontInfo[1];

            IVsFontAndColorStorage fonts = (IVsFontAndColorStorage)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsFontAndColorStorage));
            fonts.OpenCategory(new Guid(FontsAndColorsCategory.TextEditor), (uint)(__FCSTORAGEFLAGS.FCSF_LOADDEFAULTS | __FCSTORAGEFLAGS.FCSF_PROPAGATECHANGES));

            fonts.GetFont(pLOGFONT, pInfo);
            double fontSize = PointsToPixels(pInfo[0].wPointSize) * (m_zoomLevel / 100.0);
            m_textBox.FontFamily = new System.Windows.Media.FontFamily(pInfo[0].bstrFaceName);
            m_textBox.FontSize = fontSize;
            fonts.CloseCategory();
        }

        #endregion // Interface.

        static double PointsToPixels(int points)
        {
            return (points * 96.0) / 72.0;
        }

        public void RegisterForTextManagerEvents()
        {
            var textManager = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsTextManager)) as IVsTextManager2;
            var container = textManager as IConnectionPointContainer;
            IConnectionPoint textManagerEventsConnection;
            var eventGuid = typeof(IVsTextManagerEvents).GUID;
            container.FindConnectionPoint(ref eventGuid, out textManagerEventsConnection);
            uint textManagerCookie;
            textManagerEventsConnection.Advise(this, out textManagerCookie);
        }

        public void OnRegisterMarkerType(int markerType)
        {
        }

        public void OnRegisterView(IVsTextView view)
        {
        }

        public void OnUnregisterView(IVsTextView view)
        {
        }

        public void OnUserPreferencesChanged(VIEWPREFERENCES[] pViewPrefs, FRAMEPREFERENCES[] pFramePrefs, LANGPREFERENCES[] pLangPrefs, FONTCOLORPREFERENCES[] pColorPrefs)
        {
            var hash = GetHashCode();
            UpdateFont();
        }

        public void Zoom(object sender, ZoomLevelChangedEventArgs args)
        {
            m_zoomLevel = args.NewZoomLevel;
            UpdateFont();
        }
    }
}
