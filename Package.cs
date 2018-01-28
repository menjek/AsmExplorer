using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace VSAsm
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(ToolWindow))]
    [ProvideOptionPage(typeof(Options), "VSAsm", "General", 101, 106, true)]
    [Guid(PackageGuids.Package)]
    public sealed class Package : Microsoft.VisualStudio.Shell.Package
    {
        protected override void Initialize()
        {
            base.Initialize();
        }
    }
}
