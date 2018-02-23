using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace VSAsm
{
    class ViewOptions : DialogPage
    {
        [DisplayName("Label padding")]
        [Description("Number of spaces before all labels.")]
        [Category("Padding")]
        [DefaultValue(0)]
        public int LabelPadding { get; set; }

        [DisplayName("Instruction padding")]
        [Description("Starting column od instruction name.")]
        [Category("Padding")]
        [DefaultValue(4)]
        public int InstructionPadding { get; set; }

        [DisplayName("Instruction arguments padding")]
        [Description("Minimal starting column for instruction arguments.")]
        [Category("Padding")]
        [DefaultValue(16)]
        public int InstructionArgsPadding { get; set; }

        [DisplayName("Instruction arguments padding")]
        [Description("Minimal starting column for an assembly comments.")]
        [Category("Padding")]
        [DefaultValue(64)]
        public int CommentsPadding { get; set; }
    }
}