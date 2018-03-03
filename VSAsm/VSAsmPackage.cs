using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;

namespace VSAsm
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(ToolWindow))]
    [ProvideOptionPage(typeof(Options), "VSAsm", "General", 101, 106, true)]
    [ProvideOptionPage(typeof(ViewOptions), "VSAsm", "View", 101, 106, true)]
    [Guid(PackageGuids.Package)]
    public sealed class VSAsmPackage : Package
    {
        public static VSAsmPackage Instance { get; private set; } = null;

        public VSAsmPackage()
        {
            if (Instance != null) {
                ShowError("VSAsmPackage", "There are two instances of the package loaded.");
            } else {
                Instance = this;
            }
        }

        public static void ShowError(string title, string message)
        {
            VsShellUtilities.ShowMessageBox(Instance,
                title,
                message,
                OLEMSGICON.OLEMSGICON_CRITICAL,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }
    }
}
