using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
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
        TextBlock m_text = null;
        double m_zoomLevel = 1.0;
        double m_fontSize = 0.0;

        #endregion // Data.

        #region Create.

        public ToolWindowView(ToolWindow window, TextBlock text)
        {
            m_window = window;
            m_text = text;

            RegisterForTextManagerEvents();
            UpdateFont();

            TextViewCreationListener.Events += (IWpfTextView textView) => textView.ZoomLevelChanged += UpdateZoom;
        }

        #endregion // Create.

        #region Interface.

        public void OnDocumentChanged()
        {
            if (m_window.ActiveFile == null) {
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

        void SetupNoSource()
        {
            m_text.Text = "No source.";
        }

        void SetupNoAsm()
        {
            m_text.Text = "No asm.";
        }

        void SetupAsm()
        {
            m_text.Text = "";

            AsmFunction function = SearchFunction(m_window.ActiveAsm.Functions, m_window.CurrentLine);
            if (function != null) {
                SetupFunction(function);
            }
        }

        void SetupFunction(AsmFunction function)
        {
            Package package = (Package)m_window.Package;
            ViewOptions options = (ViewOptions)package.GetDialogPage(typeof(ViewOptions));
            
            m_text.Inlines.Add(new Run(function.Name + Environment.NewLine) { Foreground = Brushes.Blue, Background = Brushes.LightGreen });
            string paragraph = string.Empty;

            foreach (AsmBlock block in function.Blocks) {
                foreach (AsmInstruction instruction in block.Instructions) {
                    if (instruction.IsLabel) {
                        // We have a label.
                        paragraph += instruction.Name + Environment.NewLine;
                        continue;
                    }

                    string line = "    ";
                    line += instruction.Name;

                    if (instruction.Args != null && instruction.Args.Length != 0) {
                        line = line.PadRight(16);

                        string[] args = new string[instruction.Args.Length];
                        for (int i = 0; i < args.Length; ++i) {
                            IAsmInstructionArg arg = instruction.Args[i];
                            if (arg is AsmInstructionConstantArg) {
                                args[i] = ((AsmInstructionConstantArg)arg).Value.ToString();
                            } else if (arg is AsmInstructionRegisterArg) {
                                args[i] = ((AsmInstructionRegisterArg)arg).Name.ToString();
                            } else {
                                args[i] = ((AsmInstructionIndirectAddressArg)arg).Unparsed;
                            }
                        }

                        line += string.Join(", ", args);
                    }

                    line += Environment.NewLine;
                    paragraph += line;
                }
            }

            m_text.Inlines.Add(paragraph);
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
                m_text.FontFamily = new FontFamily(info.Value.bstrFaceName);
                m_fontSize = PointsToPixels(info.Value.wPointSize);
                m_text.FontSize = m_fontSize * m_zoomLevel;
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
            m_text.FontSize = m_fontSize * m_zoomLevel;
        }

        #endregion // Events.
    }
}
