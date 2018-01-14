using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Windows.Media;

namespace AsmExplorer
{
    class Options : DialogPage
    {
        [DisplayName("test")]
        [Description("")]
        [Category("Status")]
        //[DefaultValue()]
        public Color CompilationFailedBackground { get; set; }

        public Brush CompilationFailedText { get; set; }

        public Brush NoSourceBackground { get; set; }

        public Brush NoSourceText { get; set; }

        public Brush AssemblyBackground { get; set; }

        public Brush AssemblyText { get; set; }

        public Brush OutOfDateBackground { get; set; }

        public Brush OutOfDateText { get; set; }
    }
}