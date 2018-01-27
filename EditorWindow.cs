using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCProjectEngine;
using System;
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

        #endregion // Constants

        #region Data

        EnvDTE.DTE m_dte = null;
        EditorWindowControl m_control = null;

        #endregion // Data

        public EditorWindow() : base(null)
        {
            this.Caption = "Assembly View";

            m_control = new EditorWindowControl(this);
            this.Content = m_control;

            this.ToolBar = new CommandID(new Guid(PackageGuids.guidVSAsmWindowPackageCmdSet), PackageGuids.VSAsmToolBar);
        }

        void Test(object sender, EventArgs e)
        {
            Debugger.Break();
        }

        protected override void Initialize()
        {
            m_dte = (EnvDTE.DTE)this.GetService(typeof(EnvDTE.DTE));
            //m_dte.Events.WindowEvents.WindowActivated += OnWindowActivated;
            //m_dte.Events.WindowEvents.WindowClosing += OnWindowClosing;

            m_control.AsmText.Document.PageWidth = 1024;

            OleMenuCommandService commandService = this.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            commandService.AddCommand(new OleMenuCommand(new EventHandler(this.Test), new CommandID(new Guid(PackageGuids.guidVSAsmWindowPackageCmdSet), 0x0100)));
        }

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
                builder.WarningLevel,
                builder.WholeProgramOptimization);

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
                    Compile(compilerQuotedPath, args + " " + filePath);
                    return;
                }
            }

            OnMissingCompiler();
        }

        void Compile(string cl, string args)
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
            System.Threading.Tasks.Task.Run(() => BuildTask(process));
        }

        void BuildTask(Process process)
        {
            process.WaitForExit();

            if (process.ExitCode == 0) {
                m_control.Dispatcher.Invoke(() =>
                    this.OnCompilationSuccess()
                );
            } else {
                m_control.Dispatcher.Invoke(() =>
                    this.OnCompilationFailed()
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
        }

        void OnCompilationSuccess()
        {
            VCFile file = m_dte.ActiveDocument.ProjectItem.Object as VCFile;
            VCProject project = file.project;

            string dir = project.ActiveConfiguration.Evaluate(OutputAsmDir);
            string filename = Path.ChangeExtension(file.Name, "asm");
            string solutionDir = Path.GetDirectoryName(m_dte.Solution.FullName);

            string path = Path.Combine(solutionDir, dir, filename);
            string text = File.ReadAllText(path);

            CLAsmParser parser = new CLAsmParser();
            AsmUnit asm = parser.Parse(text);
            LoadAsm(asm);
        }

        void OnCompilationFailed()
        {
        }

        void LoadAsm(AsmUnit asm)
        {
            RichTextBox textBox = m_control.AsmText;
            
            textBox.Document.Blocks.Clear();
            textBox.Document.Blocks.Add(new Paragraph(new Run("test")));
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
