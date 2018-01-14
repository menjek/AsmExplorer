using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace AsmExplorer
{
    internal sealed class EditorWindowCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("4bfea2fa-289d-461e-982d-4fff305997c7");

        private readonly Package m_package;

        private EditorWindowCommand(Package package)
        {
            this.m_package = package;

            OleMenuCommandService commandService = ((IServiceProvider)package).GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.ShowToolWindow, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        public static EditorWindowCommand Instance
        {
            get;
            private set;
        }

        public static void Initialize(Package m_package)
        {
            Instance = new EditorWindowCommand(m_package);
        }

        private void ShowToolWindow(object sender, EventArgs e)
        {
            ToolWindowPane window = this.m_package.FindToolWindow(typeof(EditorWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}
