using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCProjectEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace VSAsm
{
    [Guid("3bf6b2bc-9c4d-41b2-8f3f-65a488653d07")]
    public class EditorWindow : ToolWindowPane
    {

        #region Constants

        private static readonly string[] CONFIGURATION_SEPARATORS = { ";" };
        private static readonly string INTERMEDIATE_DIR = "$(IntDir)";
        private static readonly string ASM_DIR = INTERMEDIATE_DIR + "/asm/";
        private static readonly string BUILD_PANE = "Build";

        #endregion

        #region Data members

        private EnvDTE.DTE m_dte = null;
        private EditorWindowControl m_control = null;

        #endregion

        public EditorWindow() : base(null)
        {
            this.Caption = "Assembly View";

            m_control = new EditorWindowControl(this);
            this.Content = m_control;

            SetNoFile();
        }

        protected override void Initialize()
        {
            m_dte = (EnvDTE.DTE)this.GetService(typeof(EnvDTE.DTE));
            //m_dte.Events.WindowEvents.WindowActivated += OnWindowActivated;
            //m_dte.Events.WindowEvents.WindowClosing += OnWindowClosing;
        }

        #region Compilation

        private static string ComposeCommandLine(VCFile file, VCCLCompilerTool cl)
        {
            CLCommandLineBuilder builder = new CLCommandLineBuilder(cl);

            string commandLine = CLCommandLineBuilder.Compose(
                // Asm generation related arguments.
                CLCommandLineBuilder.CmdCompileOnly(true),
                CLCommandLineBuilder.CmdAssemblerOutput(asmListingOption.asmListingAsmSrc),
                CLCommandLineBuilder.CmdAssemblerListingLocation(ASM_DIR),

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
            if (m_dte.ActiveDocument != null)
            {
                VCFile file = m_dte.ActiveDocument.ProjectItem.Object as VCFile;
                if (file != null)
                {
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

            string[] toolchainDirs = toolchainConfDirs.Split(CONFIGURATION_SEPARATORS,
                StringSplitOptions.RemoveEmptyEntries);

            VCCLCompilerTool cl = fileConfiguration.Tool;
            foreach (string dir in toolchainDirs)
            {
                string compilerPath = dir + "/" + cl.ToolPath;
                if (File.Exists(compilerPath))
                {
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

        private void Compile(string cl, string args)
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
            EnvDTE.OutputWindowPane buildPane = outputWindow.OutputWindowPanes.Item(BUILD_PANE);
            buildPane.Clear();
            buildPane.Activate();

            process.OutputDataReceived += (sender, eventArgs) => BuildOutputReceived(buildPane, eventArgs);

            process.Start();
            process.BeginOutputReadLine();

            OnCompilationStart();
            System.Threading.Tasks.Task.Run(() => BuildTask(process));
        }

        private void BuildTask(Process process)
        {
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                m_control.Dispatcher.Invoke(() =>
                    this.OnCompilationSuccess()
                );
            }
            else
            {
                m_control.Dispatcher.Invoke(() =>
                    this.OnCompilationFailed()
                );
            }
        }

        private void BuildOutputReceived(EnvDTE.OutputWindowPane buildPane, DataReceivedEventArgs args)
        {
            buildPane.OutputString(args.Data + Environment.NewLine);
        }

        private void EnsureAsmDirectoryExists(VCConfiguration configuration)
        {
            string relativeDir = configuration.Evaluate(ASM_DIR);
            string solutionDir = Path.GetDirectoryName(m_dte.Solution.FullName);

            string dir = Path.Combine(solutionDir, relativeDir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        #endregion

        #region States

        private void OnMissingCompiler()
        {
            m_control.AsmStatus.Content = "Failed to locate toolchain compiler.";
        }

        private void OnCompilationStart()
        {
            m_control.AsmStatus.Content = "Compiling...";
            m_control.AsmCompile.IsEnabled = false;
        }

        private void OnCompilationSuccess()
        {
            m_control.AsmCompile.IsEnabled = true;

            VCFile file = m_dte.ActiveDocument.ProjectItem.Object as VCFile;
            VCProject project = file.project;

            string dir = project.ActiveConfiguration.Evaluate(ASM_DIR);
            string filename = Path.ChangeExtension(file.Name, "asm");
            string solutionDir = Path.GetDirectoryName(m_dte.Solution.FullName);

            string path = solutionDir + "/" + dir + "/" + filename;

            string text = File.ReadAllText(path);

            CLAsmParser parser = new CLAsmParser();
            parser.Parse(text);

            m_control.AsmText.Text = text;
        }

        private void OnCompilationFailed()
        {
            m_control.AsmCompile.IsEnabled = true;
        }

        private void SetNoFile()
        {
            m_control.AsmStatus.Content = "No document opened.";
            m_control.ShowCompile();
        }

        private void SetNoSourceFile()
        {
            m_control.AsmStatus.Content = "Not a source file.";
            m_control.HideCompile();
        }

        private void SetFailedCompilation()
        {
            m_control.AsmStatus.Content = "Compilation failed.";
            m_control.ShowCompile();
        }

        private void SetAssembly(string assemblyFile)
        {
            m_control.AsmStatus.Content = "Compilation successful.";
            m_control.HideCompile();
        }

        private void SetOutOfDate()
        {
            m_control.AsmStatus.Content = "Out of date assembly.";
            m_control.ShowCompile();
        }

        #endregion

        #region Events

        private void OnWindowActivated(EnvDTE.Window focus, EnvDTE.Window lostFocus)
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

        private void OnWindowClosing(EnvDTE.Window window)
        {
        }

        private void SetupDocument(EnvDTE.Document document)
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

        #endregion
    }
}
