using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace VSAsm
{
    class Options : DialogPage
    {
        [DisplayName("Label padding")]
        [Description("Number of spaces before all labels.")]
        [Category("Assembler/Padding")]
        [DefaultValue(0)]
        public int Test { get; set; }
    }
}
