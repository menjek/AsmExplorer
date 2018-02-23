using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.VCProjectEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace VSAsm
{
    [Guid("3bf6b2bc-9c4d-41b2-8f3f-65a488653d07")]
    public class ToolWindow : ToolWindowPane
    {
        #region Constants

        static readonly string[] ConfigurationSeparators = { ";" };
        const string IntermediateDir = "$(IntDir)";
        const string OutputAsmDir = IntermediateDir + "/asm/";
        const string BuildPaneName = "Build";

        static readonly Guid WindowCommandSetGuid = new Guid(PackageGuids.WindowCommandSet);

        #endregion // Constants

        #region Data

        EnvDTE.DTE m_dte = null;
        ToolWindowControl m_control = null;
        ToolWindowView m_view = null;
        Dictionary<int, OleMenuCommand> m_commands = new Dictionary<int, OleMenuCommand>();
        Dictionary<string, AsmFile> m_asm = new Dictionary<string, AsmFile>();
        EnvDTE.Window m_activeWindow;

        #endregion // Data

        public ToolWindow() : base(null)
        {
            Caption = "Assembly View";

            m_control = new ToolWindowControl(this);
            Content = m_control;
            m_view = new ToolWindowView(this, m_control.AsmText);

            ToolBar = new CommandID(WindowCommandSetGuid, PackageGuids.Toolbar);

            TextViewCreationListener.Events += OnTextViewCreated;
        }

        public VCFile ActiveFile {
            get {
                if (m_activeWindow != null) {
                    return m_activeWindow.Document.ProjectItem.Object as VCFile;
                } else {
                    return null;
                }
            }
        }

        public IWpfTextView ActiveTextView {
            get;
            private set;
        }

        public ITextDocument ActiveTextDocument {
            get {
                if (ActiveTextView != null) {
                    ActiveTextView.TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out ITextDocument document);
                    return document;
                } else {
                    return null;
                }
            }
        }

        public AsmFile ActiveAsm {
            get {
                ITextDocument activeTextDocument = ActiveTextDocument;
                if (activeTextDocument == null) {
                    return null;
                }

                string filePath = activeTextDocument.FilePath.ToLower(); ;
                if (m_asm.TryGetValue(filePath, out AsmFile asm)) {
                    return asm;
                } else {
                    return null;
                }
            }
        }

        public int CurrentLine {
            get;
            private set;
        }

        protected override void Initialize()
        {
            m_dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
            m_dte.Events.WindowEvents.WindowActivated += OnWindowActivated;
            m_dte.Events.WindowEvents.WindowClosing += OnWindowClosing;

            RegisterCommands();
        }

        void RegisterCommands()
        {
            OleMenuCommandService service = (OleMenuCommandService)GetService(typeof(IMenuCommandService));

            RegisterCommand(service, PackageGuids.CommandShowWindow, new EventHandler(OnShow));
            RegisterCommand(service, PackageGuids.CommandCompileActive, new EventHandler(OnCompileActive));
        }

        void RegisterCommand(OleMenuCommandService service, int commandID, EventHandler handler)
        {
            CommandID command = new CommandID(WindowCommandSetGuid, commandID);
            OleMenuCommand menuItem = new OleMenuCommand(handler, command);
            m_commands[commandID] = menuItem;
            service.AddCommand(menuItem);
        }

        #region Command handlers

        void OnShow(object sender, EventArgs e)
        {
            IVsWindowFrame windowFrame = (IVsWindowFrame)Frame;
            windowFrame.Show();
        }

        void OnCompileActive(object sender, EventArgs e)
        {
            CompileActive();
        }

        #endregion // Command handlers

        #region Compilation

        static string ComposeCommandLine(VCFile file, VCCLCompilerTool cl)
        {
            CLCommandLineBuilder builder = new CLCommandLineBuilder(cl);

            string commandLine = CLCommandLineBuilder.Compose(
                // Asm generation related arguments.
                CLCommandLineBuilder.CmdCompileOnly(true),
                CLCommandLineBuilder.CmdAssemblerOutput(asmListingOption.asmListingAsmSrc),
                CLCommandLineBuilder.CmdAssemblerListingLocation(OutputAsmDir),

                // Arguments set up by user.
                builder.AdditionalIncludeDirectories,
                builder.AdditionalOptions,
                builder.AdditionalUsingDirectories,
                builder.BasicRuntimeChecks,
                builder.BufferSecurityCheck,
                builder.CallingConvention,
                builder.CompileAs,
                builder.DefaultCharIsUnsigned,
                builder.Detect64BitPortabilityProblems,
                builder.DisableLanguageExtensions,
                builder.DisableSpecificWarnings,
                builder.EnableEnhancedInstructionSet,
                builder.EnableFiberSafeOptimizations,
                builder.EnableFunctionLevelLinking,
                builder.EnableIntrinsicFunctions,
                builder.EnablePREfast,
                builder.ErrorReporting,
                builder.ExceptionHandling,
                builder.ExpandAttributedSource,
                builder.FavorSizeOrSpeed,
                builder.FloatingPointExceptions,
                builder.FloatingPointModel,
                builder.ForceConformanceInForLoopScope,
                builder.ForcedIncludeFiles,
                builder.ForcedUsingFiles,
                builder.FullIncludePath,
                builder.IgnoreStandardIncludePath,
                builder.InlineFunctionExpansion,
                builder.OmitFramePointers,
                builder.OmitDefaultLibName,
                builder.OpenMP,
                builder.Optimization,
                builder.PrecompiledHeaderFile,
                builder.PreprocessorDefinitions,
                builder.RuntimeLibrary,
                builder.RuntimeTypeInfo,
                builder.SmallerTypeCheck,
                builder.StringPooling,
                builder.StructMemberAlignment,
                builder.TreatWChar_tAsBuiltInType,
                builder.UndefineAllPreprocessorDefinitions,
                builder.UndefinePreprocessorDefinitions,
                builder.UseFullPaths,
                builder.UsePrecompiledHeader,
                builder.WarnAsError,
                builder.WarningLevel);

            commandLine = file.project.ActiveConfiguration.Evaluate(commandLine);
            commandLine = commandLine.Replace("\\", "\\\\");

            return commandLine;
        }

        public void CompileActive()
        {
            if (m_dte.ActiveDocument != null) {
                if (m_dte.ActiveDocument.ProjectItem.Object is VCFile file) {
                    Compile(file);
                }
            }
        }

        public void Compile(VCFile file)
        {
            VCConfiguration configuration = file.project.ActiveConfiguration;
            VCFileConfiguration fileConfiguration = file.GetFileConfigurationForProjectConfiguration(configuration);

            string toolchainConfDirs = configuration.Platform.ExecutableDirectories;
            toolchainConfDirs = configuration.Evaluate(toolchainConfDirs);

            string[] toolchainDirs = toolchainConfDirs.Split(ConfigurationSeparators,
                StringSplitOptions.RemoveEmptyEntries);

            VCCLCompilerTool cl = fileConfiguration.Tool;
            foreach (string dir in toolchainDirs) {
                string compilerPath = Path.Combine(dir, cl.ToolPath);
                if (File.Exists(compilerPath)) {
                    EnsureAsmDirectoryExists(configuration);

                    string compilerQuotedPath = CLCommandLineBuilder.SurroundWithQuotes(compilerPath);
                    string filePath = CLCommandLineBuilder.SurroundWithQuotes(file.FullPath);
                    string args = ComposeCommandLine(file, cl);
                    Compile(compilerQuotedPath, args + " " + filePath, file);
                    return;
                }
            }

            OnMissingCompiler();
        }

        void Compile(string cl, string args, VCFile file)
        {
            Process process = new Process();
            process.StartInfo.FileName = cl;
            process.StartInfo.Arguments = args;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(m_dte.Solution.FullName);

            EnvDTE.Window window = m_dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
            EnvDTE.OutputWindow outputWindow = (EnvDTE.OutputWindow)window.Object;
            EnvDTE.OutputWindowPane buildPane = outputWindow.OutputWindowPanes.Item(BuildPaneName);
            buildPane.Clear();
            buildPane.Activate();

            process.OutputDataReceived += (sender, eventArgs) => BuildOutputReceived(buildPane, eventArgs);

            process.Start();
            process.BeginOutputReadLine();

            OnCompilationStart();
            System.Threading.Tasks.Task.Run(() => BuildTask(process, file));
        }

        void BuildTask(Process process, VCFile file)
        {
            process.WaitForExit();

            if (process.ExitCode == 0) {
                m_control.Dispatcher.Invoke(() =>
                    OnCompilationSuccess(file)
                );
            } else {
                m_control.Dispatcher.Invoke(() =>
                    OnCompilationFailed()
                );
            }
        }

        void BuildOutputReceived(EnvDTE.OutputWindowPane buildPane, DataReceivedEventArgs args)
        {
            buildPane.OutputString(args.Data + Environment.NewLine);
        }

        void EnsureAsmDirectoryExists(VCConfiguration configuration)
        {
            string relativeDir = configuration.Evaluate(OutputAsmDir);
            string solutionDir = Path.GetDirectoryName(m_dte.Solution.FullName);

            string dir = Path.Combine(solutionDir, relativeDir);
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
        }

        #endregion // Compilation

        #region States

        void OnMissingCompiler()
        {
        }

        void OnCompilationStart()
        {
            IVsStatusbar statusBar = (IVsStatusbar)GetService(typeof(SVsStatusbar));
            statusBar.SetText("Asm compilation started.");
            object icon = (short)Constants.SBAI_Build;
            statusBar.Animation(1, ref icon);
            m_commands[PackageGuids.CommandCompileActive].Enabled = false;
        }

        void OnCompilationEnd(string status)
        {
            IVsStatusbar statusBar = (IVsStatusbar)GetService(typeof(SVsStatusbar));
            object icon = (short)Constants.SBAI_Build;
            statusBar.Animation(0, ref icon);
            statusBar.SetText(status);
            m_commands[PackageGuids.CommandCompileActive].Enabled = true;
        }

        void OnCompilationSuccess(VCFile file)
        {
            OnCompilationEnd("Asm compilation successful.");

            VCProject project = file.project;

            string dir = project.ActiveConfiguration.Evaluate(OutputAsmDir);
            string filename = Path.ChangeExtension(file.Name, "asm");
            string solutionDir = Path.GetDirectoryName(m_dte.Solution.FullName);

            string path = Path.Combine(solutionDir, dir, filename);
            string text = File.ReadAllText(path);

            CLAsmParser parser = new CLAsmParser();
            AsmUnit asmUnit = parser.Parse(text);

            foreach (KeyValuePair<string, AsmFile> filePair in asmUnit.Files) {
                string fileName = filePair.Key;
                AsmFile asmFile = filePair.Value;

                if (m_asm.ContainsKey(fileName)) {
                    UpdateFile(m_asm[fileName], asmFile);
                } else {
                    m_asm[fileName] = asmFile;
                }
            }

            if (ActiveFile == file) {
                m_view.OnDocumentChanged();
            }
        }

        void UpdateFile(AsmFile current, AsmFile newFile)
        {
            current.Functions = newFile.Functions;
        }

        void OnCompilationFailed()
        {
            OnCompilationEnd("Asm compilation failed.");
        }

        #endregion // States

        #region Events

        void OnWindowActivated(EnvDTE.Window gotFocus, EnvDTE.Window lostFocus)
        {
            if (gotFocus.Type != EnvDTE.vsWindowType.vsWindowTypeDocument) {
                return;
            }

            CurrentLine = 0;

            VCFile file = gotFocus.Document.ProjectItem.Object as VCFile;
            bool isSource = (file != null) && (file.FileType == eFileType.eFileTypeCppCode);
            if (isSource) {
                m_activeWindow = gotFocus;
            } else {
                m_activeWindow = null;
            }

            m_view.OnDocumentChanged();
        }

        void OnWindowClosing(EnvDTE.Window window)
        {
            if (window.Type != EnvDTE.vsWindowType.vsWindowTypeDocument) {
                return;
            }

            if (window == m_activeWindow) {
                m_activeWindow = null;
                CurrentLine = 0;
                m_view.OnDocumentChanged();
            }
        }

        void OnTextViewCreated(IWpfTextView textView)
        {
            textView.GotAggregateFocus += OnTextViewGotFocus;
            textView.LostAggregateFocus += OnTextViewLostFocus;
            textView.Caret.PositionChanged += OnTextViewCaretPositionChanged;

            ITextDocument textDocument = textView.TextBuffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument));
            textDocument.DirtyStateChanged += OnDirtyStateChanged;
        }

        void OnTextViewGotFocus(object sender, EventArgs args)
        {
            ActiveTextView = (IWpfTextView)sender;
            UpdateLineNumber(ActiveTextView);
            m_view.OnDocumentChanged();
        }

        void OnTextViewLostFocus(object sender, EventArgs args)
        {
            ActiveTextView = null;
        }

        void OnTextViewCaretPositionChanged(object sender, CaretPositionChangedEventArgs args)
        {
            UpdateLineNumber(args.TextView);
        }

        void OnDirtyStateChanged(object sender, EventArgs args)
        {
            ITextDocument textDocument = (ITextDocument)sender;
            m_view.OnDirtyStateChanged(textDocument.IsDirty);
        }

        #endregion // Events

        void UpdateLineNumber(ITextView textView)
        {
            CaretPosition caretPosition = textView.Caret.Position;
            SnapshotPoint? point = caretPosition.Point.GetPoint(textView.TextBuffer, caretPosition.Affinity);
            if (point.HasValue) {
                int lineNumber = point.Value.GetContainingLine().LineNumber + 1;
                if (lineNumber != CurrentLine) {
                    CurrentLine = lineNumber;
                    m_view.OnLineChanged();
                }
            }
        }
    }
}
