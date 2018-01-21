using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace AsmExplorer
{
    class CLAsmParser
    {
        #region Constants

        private static readonly string FILE_SECTION_START = "; File";
        private static readonly string FUNCTION_SECTION_START = "PROC";
        private static readonly string FUNCTION_SECTION_END = "ENDP";
        private static readonly string FUNCTION_SIGNATURE_END = ", COMDAT";
        private static readonly string[] LINE_SEPARATORS = { "\n", "\r\n" };

        #endregion // Constants

        #region Data

        private class AsmFileBuilder
        {
            public string Path { get; set; }
            public List<AsmFunction> Functions { get; set; }
        }

        private string[] m_lines = null;
        private int m_currentLine = 0;

        private string CurrentLine
        {
            get { return m_lines[m_currentLine]; }
        }

        #endregion // Data

        #region Parse interface

        public AsmUnit ParseFile(string file)
        {
            try
            {
                string[] lines = File.ReadAllLines(file);
                return Parse(lines);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public AsmUnit Parse(string text)
        {
            return Parse(text.Split(LINE_SEPARATORS, StringSplitOptions.RemoveEmptyEntries));
        }

        public AsmUnit Parse(string[] lines)
        {
            m_lines = lines;
            m_currentLine = 0;
            ParseUnit();
            return null;
        }

        #endregion // Parse interface

        #region Parse implementation

        private string NextLine()
        {
            return m_lines[++m_currentLine];
        }

        private void ParseUnit()
        {
            Dictionary<string, AsmFileBuilder> files = new Dictionary<string, AsmFileBuilder>();

            AsmFileBuilder fileBuilder = null;
            while (m_currentLine + 1 < m_lines.Length)
            {
                string line = NextLine();
                if (line.StartsWith(FILE_SECTION_START))
                {
                    string path = line.Substring(FILE_SECTION_START.Length + 1);
                    if (!files.TryGetValue(path, out fileBuilder))
                    {
                        fileBuilder = new AsmFileBuilder() {
                            Path = path,
                            Functions = new List<AsmFunction>()
                        };
                        files.Add(path, fileBuilder);
                    }
                }
                else if (line.IndexOf(FUNCTION_SECTION_START) != -1)
                {
                    if (fileBuilder != null)
                    {
                        fileBuilder.Functions.Add(ParseFunction());
                    }
                }
            }

            return;
        }

        private AsmFunction ParseFunction()
        {
            AsmFunction function = ParseFunctionSignature(CurrentLine);

            NextLine();
            List<AsmBlock> blocks = new List<AsmBlock>();
            while (!CurrentLine.Contains(FUNCTION_SECTION_END))
            {
                blocks.Add(ParseBlock());
            }

            function.Blocks = blocks.ToArray();
            return function;
        }

        private AsmFunction ParseFunctionSignature(string signature)
        {
            AsmFunction function = new AsmFunction();

            int keywordStart = signature.IndexOf(FUNCTION_SECTION_START);
            string mangledName = signature.Substring(0, keywordStart);
            function.MangledName = mangledName.TrimEnd();

            int nameStart = signature.IndexOf(';', keywordStart) + 1;
            if (nameStart != -1)
            {
                int nameEnd = signature.IndexOf(FUNCTION_SIGNATURE_END, nameStart);
                if (nameEnd != -1)
                {
                    function.Name = signature.Substring(nameStart, nameEnd - nameStart);
                }
                else
                {
                    function.Name = signature.Substring(nameStart);
                }
            }
            else
            {
                function.Name = function.MangledName;
            }

            return function;
        }

        private AsmBlock ParseBlock()
        {
            AsmBlock block = new AsmBlock();

            if (CurrentLine[0] == ';')
            {
                SkipEmptyLines();

                // Expect line starting with semicolon.
                // Such lines refere to source code.
                Debug.Assert(CurrentLine[0] == ';');

                block.SourceStartLine = ParseSourceLineNumber(CurrentLine);

                string lastSourceLine = null;
                while (CurrentLine[0] == ';')
                {
                    lastSourceLine = CurrentLine;
                    NextLine();
                }

                block.SourceEndLine = ParseSourceLineNumber(lastSourceLine);

                SkipEmptyLines();

                // Expect at least one assembly line, line not starting
                // with semicolon and not an end of a function.

                Debug.Assert(CurrentLine[0] != ';');
                Debug.Assert(!CurrentLine.Contains(FUNCTION_SECTION_END));

                List<string> assembly = new List<string>();
                while (!IsEndOfAssembly(CurrentLine))
                {
                    assembly.Add(CurrentLine);
                    NextLine();
                }

                block.Assembly = assembly.ToArray();

            }
            else
            {
                block.SourceStartLine = -1;
                block.SourceEndLine = -1;

                List<string> assembly = new List<string>();
                while (!CurrentLine.Contains(FUNCTION_SECTION_END))
                {
                    assembly.Add(CurrentLine);
                    NextLine();
                }

                block.Assembly = assembly.ToArray();
            }

            return block;
        }

        private bool IsEndOfAssembly(string line)
        {
            return string.IsNullOrWhiteSpace(line) ||
                (line[0] == ';') ||
                (line.IndexOf(FUNCTION_SECTION_END) != -1);
        }

        private int ParseSourceLineNumber(string line)
        {
            const int NUMBER_START = 2;
            int numberEnd = line.IndexOf(' ', NUMBER_START);
            string lineNumberText = line.Substring(NUMBER_START, numberEnd - NUMBER_START);
            return int.Parse(lineNumberText);
        }

        private void SkipEmptyLines()
        {
            while (string.IsNullOrWhiteSpace(CurrentLine))
                NextLine();
        }

        #endregion // Parse implementation
    }
}
