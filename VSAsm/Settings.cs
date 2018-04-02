using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace VSAsm
{
    [DataContract]
    class Settings
    {
        #region Constants

        const string DirectoryName = "VSAsm";
        const string FileName = "Settings.json";

        #endregion // Constants

        static Settings m_instance = null;

        #region Load/Save

        static string GetDirectory()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(path, DirectoryName);
        }

        static string GetPath()
        {
            return Path.Combine(GetDirectory(), FileName);
        }

        public static Settings Load()
        {
            if (m_instance == null) {
                m_instance = LoadFromFile();
            }

            return m_instance;
        }

        static Settings LoadFromFile()
        {
            try {
                return LoadFromFile(GetPath());
            } catch {
                return new Settings();
            }
        }

        static Settings LoadFromFile(string path)
        {
            using (FileStream stream = File.OpenRead(path)) {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Settings));
                return (Settings)serializer.ReadObject(stream);
            }
        }

        public void Save()
        {
            try {
                Directory.CreateDirectory(GetDirectory());
                SaveToFile(GetPath());
            } catch (Exception e) {
                VSAsmPackage.ShowError("VSAsm save settings",
                    "Failed to save the extension settings to storage: " + e.Message);
            }
        }

        void SaveToFile(string path)
        {
            using (FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write)) {
                using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, true, true, "    ")) {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Settings));
                    serializer.WriteObject(writer, this);
                    writer.Flush();
                }
            }
        }

        #endregion // Load/Save

        #region View

        public enum MatchingLinesPresets
        {
            Light,
            Blue,
            Dark,
            Custom
        }

        [DataMember]
        public int LabelPadding { get; set; } = 0;
        [DataMember]
        public int InstructionPadding { get; set; } = 4;
        [DataMember]
        public int InstructionArgsPadding { get; set; } = 16;
        [DataMember]
        public int CommentPadding { get; set; } = 64;

        [DataMember]
        public MatchingLinesPresets MatchingLinesPreset { get; set; }
        [DataMember]
        public Color[] MatchingLinesCustomColors { get; set; }

        #endregion // View
    }
}
