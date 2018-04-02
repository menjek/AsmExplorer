using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Drawing;

namespace VSAsm
{
    sealed class ViewOptions : DialogPage
    {
        #region Serialization

        private Settings m_settings = null;

        public override void LoadSettingsFromStorage()
        {
            m_settings = Settings.Load();
        }

        public override void SaveSettingsToStorage()
        {
            m_settings.Save();
        }

        #endregion // Serialization

        #region Padding

        [DisplayName("Label")]
        [Description("Number of spaces before all labels.")]
        [Category("Padding")]
        [DefaultValue(0)]
        public int LabelPadding {
            get => m_settings.LabelPadding;
            set => m_settings.LabelPadding = value;
        }

        [DisplayName("Instruction")]
        [Description("Starting column of an instruction name.")]
        [Category("Padding")]
        [DefaultValue(4)]
        public int InstructionPadding {
            get => m_settings.InstructionPadding;
            set => m_settings.InstructionPadding = value;
        }

        [DisplayName("Instruction arguments")]
        [Description("Minimal starting column for instruction arguments.")]
        [Category("Padding")]
        [DefaultValue(16)]
        public int InstructionArgsPadding {
            get => m_settings.InstructionArgsPadding;
            set => m_settings.InstructionArgsPadding = value;
        }

        [DisplayName("Instruction comment")]
        [Description("Minimal starting column for assembly comments.")]
        [Category("Padding")]
        [DefaultValue(64)]
        public int CommentPadding {
            get => m_settings.CommentPadding;
            set => m_settings.CommentPadding = value;
        }

        #endregion // Padding

        #region Colors

        static readonly Color[] MatchingLinesLightPreset =
        {
            Color.FromArgb(255, 128, 128),
            Color.FromArgb(128, 255, 128),
            Color.FromArgb(128, 128, 255),
            Color.FromArgb(255, 255, 128),
            Color.FromArgb(128, 255, 255)
        };

        static readonly Color[] MatchingLinesBluePreset =
        {
            Color.Green,
            Color.Green,
            Color.Green,
            Color.Green,
            Color.Green
        };

        static readonly Color[] MatchingLinesDarkPreset =
        {
            Color.Blue,
            Color.Blue,
            Color.Blue,
            Color.Blue,
            Color.Blue
        };

        [DisplayName("Preset")]
        [Category("Matching Lines")]
        public Settings.MatchingLinesPresets MatchingLinesPreset {
            get => m_settings.MatchingLinesPreset;
            set => m_settings.MatchingLinesPreset = value;
        }

        [DisplayName("Custom Colors")]
        [Category("Matching Lines")]
        public Color[] MatchingLinesCustomColors {
            get => m_settings.MatchingLinesCustomColors;
            set => m_settings.MatchingLinesCustomColors = value;
        }

        #endregion // Colors
    }
}