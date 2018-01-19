using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCProjectEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace AsmExplorer
{
    [Guid("3bf6b2bc-9c4d-41b2-8f3f-65a488653d07")]
    public class EditorWindow : ToolWindowPane
    {
        private EnvDTE.DTE m_dte = null;
        private EditorWindowControl m_control = null;

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

        private string ComposeCommandLine(VCFile file, VCCLCompilerTool cl)
        {
            CLCommandLineBuilder commandLine = new CLCommandLineBuilder(cl);

            string args = CLCommandLineBuilder.Compose(
                // Asm generation related arguments.
                CLCommandLineBuilder.CmdCompileOnly(true),
                CLCommandLineBuilder.CmdAssemblerOutput(asmListingOption.asmListingAsmSrc),
                CLCommandLineBuilder.CmdAssemblerListingLocation("$(IntDir)"),

                // Arguments set up by user.
                commandLine.AdditionalIncludeDirectories,
                commandLine.AdditionalOptions,
                commandLine.AdditionalUsingDirectories,
                commandLine.BasicRuntimeChecks,
                commandLine.BufferSecurityCheck,
                commandLine.CallingConvention,
                commandLine.CompileAs,
                commandLine.DefaultCharIsUnsigned,
                commandLine.Detect64BitPortabilityProblems,
                commandLine.DisableLanguageExtensions,
                commandLine.DisableSpecificWarnings,
                commandLine.EnableEnhancedInstructionSet,
                commandLine.EnableFiberSafeOptimizations,
                commandLine.EnableFunctionLevelLinking,
                commandLine.EnableIntrinsicFunctions,
                commandLine.EnablePREfast,
                commandLine.ErrorReporting,
                commandLine.ExceptionHandling,
                commandLine.ExpandAttributedSource,
                commandLine.FavorSizeOrSpeed,
                commandLine.FloatingPointExceptions,
                commandLine.FloatingPointModel,
                commandLine.ForceConformanceInForLoopScope,
                commandLine.ForcedIncludeFiles,
                commandLine.ForcedUsingFiles,
                commandLine.FullIncludePath,
                commandLine.IgnoreStandardIncludePath,
                commandLine.InlineFunctionExpansion,
                commandLine.OmitFramePointers,
                commandLine.OmitDefaultLibName,
                commandLine.OpenMP,
                commandLine.Optimization,
                commandLine.PrecompiledHeaderFile,
                commandLine.PreprocessorDefinitions,
                commandLine.RuntimeLibrary,
                commandLine.RuntimeTypeInfo,
                commandLine.SmallerTypeCheck,
                commandLine.StringPooling,
                commandLine.StructMemberAlignment,
                commandLine.TreatWChar_tAsBuiltInType,
                commandLine.UndefineAllPreprocessorDefinitions,
                commandLine.UndefinePreprocessorDefinitions,
                commandLine.UseFullPaths,
                commandLine.UsePrecompiledHeader,
                commandLine.WarnAsError,
                commandLine.WarningLevel,
                commandLine.WholeProgramOptimization);

            args = file.project.ActiveConfiguration.Evaluate(args);
            args = args.Replace("\\", "\\\\");

            return args;
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
            VCProject project = file.project;
            VCConfiguration projectConfiguration = project.ActiveConfiguration;
            VCFileConfiguration fileConfiguration = file.GetFileConfigurationForProjectConfiguration(projectConfiguration);

            string joinedDirectories = projectConfiguration.Evaluate(projectConfiguration.Platform.ExecutableDirectories);
            string[] directories = joinedDirectories.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            VCCLCompilerTool cl = fileConfiguration.Tool;

            foreach (string directory in directories)
            {
                string compilerPath = directory + "/" + cl.ToolPath;
                if (File.Exists(compilerPath))
                {
                    compilerPath = "\"" + Path.GetFullPath(compilerPath) + "\"";
                    string filePath = "\"" + file.FullPath + "\"";

                    string args = ComposeCommandLine(file, cl);

                    Compile(compilerPath, args + " " + filePath);
                    return;
                }
            }

            // Failed to locate compiler.
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
            EnvDTE.OutputWindowPane buildPane = outputWindow.OutputWindowPanes.Item("Build");
            buildPane.Clear();
            buildPane.Activate();

            process.OutputDataReceived += (sender, eventArgs) => OutputReceived(buildPane, eventArgs);

            process.Start();
            process.BeginOutputReadLine();

            System.Threading.Tasks.Task.Run(() => BuildTask(process));
        }

        private void BuildTask(Process process)
        {
            // Set compiling...

            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                // Success.
                m_control.Dispatcher.Invoke(() =>
                    this.OnCompilationSuccess()
                );
            }
            else
            {
                // Compilation failed.
                m_control.Dispatcher.Invoke(() =>
                    this.OnCompilationFailed()
                );
            }
        }

        private void OnCompilationSuccess()
        {
            VCFile file = m_dte.ActiveDocument.ProjectItem.Object as VCFile;
            VCProject project = file.project;

            string dir = project.ActiveConfiguration.Evaluate("$(IntDir)");
            string filename = Path.ChangeExtension(file.Name, "asm");
            string solutionDir  = Path.GetDirectoryName(m_dte.Solution.FullName);

            string path = solutionDir + "/" + dir + "/" + filename;

            string text = File.ReadAllText(path);

            m_control.AsmText.Text = text;
        }

        private void OnCompilationFailed()
        {
        }

        private void OutputReceived(EnvDTE.OutputWindowPane buildPane, DataReceivedEventArgs args)
        {
            buildPane.OutputString(args.Data + Environment.NewLine);
        }

        #endregion

        #region States

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
