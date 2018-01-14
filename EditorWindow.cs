using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCProjectEngine;
using System;
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
            var engine = project.VCProjectEngine.Platforms;

            VCConfiguration debugConfig = project.Configurations.Item(1);
            VCConfiguration releaseConfig = project.Configurations.Item(2);
            var sheet = project.ActiveConfiguration.PropertySheets.Item(1);
            // platform > executable directories.

            var dir = debugConfig.Evaluate(debugConfig.Platform.ExecutableDirectories);

            IVCCollection configurations = (IVCCollection)file.FileConfigurations;
            VCFileConfiguration configuration = (VCFileConfiguration)configurations.Item(2);

            VCCLCompilerTool cl = (VCCLCompilerTool)configuration.Tool;
            Compiler.ComposeCommandLine(cl);

            //cl.AssemblerOutput = asmListingOption.asmListingAsmSrc;
            //configuration.Compile(false, false);
            //cl.AssemblerOutput = asmListingOption.asmListingNone;
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
