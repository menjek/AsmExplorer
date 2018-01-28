using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.VCProjectEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Documents;

namespace VSAsm
{
    [Guid("3bf6b2bc-9c4d-41b2-8f3f-65a488653d07")]
    public class EditorWindow : ToolWindowPane
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
        EditorWindowControl m_control = null;
        Dictionary<int, OleMenuCommand> m_commands = new Dictionary<int, OleMenuCommand>();
        Dictionary<VCFile, AsmUnit> m_asm = new Dictionary<VCFile, AsmUnit>();

        #endregion // Data

        public EditorWindow() : base(null)
        {
            Caption = "Assembly View";

            m_control = new EditorWindowControl(this);
            Content = m_control;

            ToolBar = new CommandID(WindowCommandSetGuid, PackageGuids.Toolbar);
        }

        protected override void Initialize()
        {
            m_dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
            //m_dte.Events.WindowEvents.WindowActivated += OnWindowActivated;
            //m_dte.Events.WindowEvents.WindowClosing += OnWindowClosing;

            m_control.AsmText.Document.PageWidth = 1024;

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
            AsmUnit asm = parser.Parse(text);
            m_asm[file] = asm;

            if (m_dte.ActiveDocument.ProjectItem.Object as VCFile == file) {
                LoadAsm(asm);
            }
        }

        void OnCompilationFailed()
        {
            OnCompilationEnd("Asm compilation failed.");
        }

        void LoadAsm(AsmUnit asm)
        {
            RichTextBox textBox = m_control.AsmText;

            textBox.Document.Blocks.Clear();
            textBox.AppendText("test0\ntest1\ntest2");
        }

        #endregion // States

        #region Events

        void OnWindowActivated(EnvDTE.Window focus, EnvDTE.Window lostFocus)
        {
            //if (focus.Kind == WindowDocumentKind)
            //{
            //    if (m_lastDocument == focus.Document)
            //    {
            //        return;
            //    }

            //    m_lastDocument = focus.Document;
            //    SetupDocument(focus.Document);
            //}
        }

        void OnWindowClosing(EnvDTE.Window window)
        {
        }

        void SetupDocument(EnvDTE.Document document)
        {
            //VCFile file = document.ProjectItem.Object as VCFile;

            //bool isSource = (file != null) && (file.FileType == eFileType.eFileTypeCppCode);
            //if (isSource)
            //{
            //    Compile(file);
            //}
            //else
            //{
            //    FileAsmWindow.SetNoSourceFile();
            //}
        }

        #endregion // Events
    }
}
