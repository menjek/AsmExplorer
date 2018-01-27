using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace VSAsm
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(EditorWindow))]
    [ProvideOptionPage(typeof(Options), "VSAsm", "General", 101, 106, true)]
    [Guid(Package.PackageGuidString)]
    public sealed class Package : Microsoft.VisualStudio.Shell.Package
    {
        public const string PackageGuidString = "13e6928c-0bed-4914-8f5f-dfc6556a47dc";

        protected override void Initialize()
        {
            base.Initialize();
        }
    }
}
