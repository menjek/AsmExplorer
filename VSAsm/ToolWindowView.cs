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
        private const string V = "No asm.";
        static readonly Guid TextEditorFontGuid = new Guid(FontsAndColorsCategory.TextEditor);

        #endregion // Constants.

        #region Colors

        static readonly Brush FUNCTION_FOREGROUND = Brushes.DarkRed;
        static readonly Brush LABEL_FOREGROUND = Brushes.DarkBlue;
        static readonly Brush INSTRUCTION_NAME_FOREGROUND = Brushes.Black;
        static readonly Brush CONSTANT_FOREGROUND = Brushes.Blue;
        static readonly Brush REGISTER_FOREGROUND = Brushes.Black;
        static readonly Brush COMMENT_FOREGROUND = Brushes.Green;

        #endregion // Colors

        #region Data.

        ToolWindow m_window = null;
        TextBlock m_text = null;
        double m_zoomLevel = 1.0;
        double m_fontSize = 0.0;
        ViewOptions m_viewOptions = null;
        AsmFunctionDecorator m_decoratedFunction = null;

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
            m_text.Text = V;
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
            if (m_viewOptions == null) {
                m_viewOptions = (ViewOptions)VSAsmPackage.Instance.GetDialogPage(typeof(ViewOptions));
            }

            AddFunctionHeader(function);

            int index = 0;
            foreach (AsmBlock block in function.Blocks) {
                AddBlock(index, block);
                ++index;
            }
        }

        static void AddPadding(InlineCollection inlines, int padding)
        {
            if (padding != 0) {
                inlines.Add(new Run(new string(' ', padding)));
            }
        }

        void AddFunctionHeader(AsmFunction function)
        {
            m_decoratedFunction = new AsmFunctionDecorator() {
                Function = function,
                Run = new Run(function.Name) { Foreground = FUNCTION_FOREGROUND },
                Blocks = new List<AsmBlockDecorator>()
            };

            m_text.Inlines.Add(m_decoratedFunction.Run);
            m_text.Inlines.Add(new Run(Environment.NewLine));
        }

        void AddBlock(int index, AsmBlock block)
        {
            Span blockSpan = new Span();

            AsmBlockDecorator decoratedBlock = new AsmBlockDecorator() {
                Block = block,
                Span = blockSpan,
                Instructions = new List<AsmInstructionDecorator>()
            };

            m_decoratedFunction.Blocks.Add(decoratedBlock);

            foreach (AsmInstruction instruction in block.Instructions) {
                AsmInstructionDecorator decoratedInstruction = null;

                if (instruction.IsLabel) {
                    AddPadding(blockSpan.Inlines, m_viewOptions.LabelPadding);
                    decoratedInstruction = CreateLabel(instruction);
                } else {
                    AddPadding(blockSpan.Inlines, m_viewOptions.InstructionPadding);
                    decoratedInstruction = CreateInstruction(instruction);
                }

                decoratedBlock.Instructions.Add(decoratedInstruction);
                blockSpan.Inlines.Add(decoratedInstruction.Span);
                blockSpan.Inlines.Add(new Run(Environment.NewLine));
            }

            m_text.Inlines.Add(blockSpan);
        }

        AsmInstructionDecorator CreateLabel(AsmInstruction label)
        {
            AsmInstructionDecorator decorator = new AsmInstructionDecorator() {
                Instruction = label,
                Span = new Span(),
                Name = new Run(label.Name) { Foreground = LABEL_FOREGROUND }
            };

            decorator.Span.Inlines.Add(decorator.Name);
            return decorator;
        }

        AsmInstructionDecorator CreateInstruction(AsmInstruction instruction)
        {
            AsmInstructionDecorator decorator = new AsmInstructionDecorator() {
                Instruction = instruction,
                Span = new Span(),
                Name = CreateInstructionName(instruction),
                Args = CreateInstructionArgs(instruction),
                Comment = CreateInstructionComment(instruction)
            };

            int column = m_viewOptions.InstructionPadding;

            decorator.Span.Inlines.Add(decorator.Name);
            column += instruction.Name.Length;

            if (column < m_viewOptions.InstructionArgsPadding) {
                string padding = new string(' ', m_viewOptions.InstructionArgsPadding - column);
                decorator.Span.Inlines.Add(new Run(padding));
                column = m_viewOptions.InstructionArgsPadding;
            }

            if (decorator.Args != null) {
                for (int i = 0; i < decorator.Args.Count; ++i) {
                    AsmInstructionArgDecorator arg = decorator.Args[i];
                    decorator.Span.Inlines.Add(arg.Run);
                    column += arg.Run.Text.Length;

                    if ((i + 1) != decorator.Args.Count) {
                        decorator.Span.Inlines.Add(new Run(", "));
                        column += 2;
                    }
                }
            }

            if (decorator.Comment != null) {
                if (column < m_viewOptions.CommentPadding) {
                    string padding = new string(' ', m_viewOptions.CommentPadding - column);
                    decorator.Span.Inlines.Add(new Run(padding));
                }

                decorator.Span.Inlines.Add(decorator.Comment);
            }

            return decorator;
        }

        Run CreateInstructionName(AsmInstruction instruction)
        {
            return new Run(instruction.Name) { Foreground = INSTRUCTION_NAME_FOREGROUND };
        }

        Run CreateInstructionComment(AsmInstruction instruction)
        {
            Run comment = null;
            if (instruction.Comment != null) {
                comment = new Run("; " + instruction.Comment) { Foreground = COMMENT_FOREGROUND };
            }
            return comment;
        }

        List<AsmInstructionArgDecorator> CreateInstructionArgs(AsmInstruction instruction)
        {
            List<AsmInstructionArgDecorator> args = null;

            if (instruction.Args != null) {
                args = new List<AsmInstructionArgDecorator>();

                foreach (IAsmInstructionArg arg in instruction.Args) {
                    AsmInstructionArgDecorator decoratedArg = new AsmInstructionArgDecorator() {
                        Arg = arg,
                        Run = CreateInstructionArg(arg)
                    };
                    args.Add(decoratedArg);
                }
            }

            return args;
        }

        Run CreateInstructionArg(IAsmInstructionArg arg)
        {
            Run run = null;
            switch (arg.Type) {
                case InstructionArgType.Constant: {
                    AsmInstructionConstantArg constant = (AsmInstructionConstantArg)arg;
                    run = new Run(constant.Value.ToString()) { Foreground = CONSTANT_FOREGROUND };
                    break;
                }
                case InstructionArgType.IndirectAddress: {
                    AsmInstructionIndirectAddressArg indirect = (AsmInstructionIndirectAddressArg)arg;
                    run = new Run(indirect.Unparsed);
                    break;
                }
                case InstructionArgType.Register: {
                    AsmInstructionRegisterArg register = (AsmInstructionRegisterArg)arg;
                    run = new Run(register.Name) { Foreground = REGISTER_FOREGROUND };
                    break;
                }
            }
            return run;
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
            IConnectionPointContainer container = (IConnectionPointContainer)VSAsmPackage.GetGlobalService(typeof(SVsTextManager));
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
            IVsFontAndColorStorage fontStorage = (IVsFontAndColorStorage)VSAsmPackage.GetGlobalService(typeof(SVsFontAndColorStorage));
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
