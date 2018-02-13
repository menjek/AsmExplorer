using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace VSAsm
{
    class CLAsmParser
    {
        #region Constants

        const string FileStartID = "; File";
        const string FunctionStartID = "PROC";
        const string FunctionEndID = "ENDP";
        const string FunctionCOMDAT = "COMDAT";
        static readonly string[] NewLines = { "\n", "\r\n" };

        #endregion // Constants

        #region Data

        string[] m_lines = null;
        int m_currentLine = 0;

        string CurrentLine {
            get { return m_lines[m_currentLine]; }
        }

        bool IsEOF {
            get { return ((m_currentLine + 1) == m_lines.Length); }
        }

        #endregion // Data

        #region Parse interface

        public AsmUnit ParseFile(string file)
        {
            string[] lines = File.ReadAllLines(file);
            return Parse(lines);
        }

        public AsmUnit Parse(string text)
        {
            string[] lines = text.Split(NewLines, StringSplitOptions.RemoveEmptyEntries);
            return Parse(lines);
        }

        public AsmUnit Parse(string[] lines)
        {
            m_lines = lines;
            m_currentLine = 0;
            return ParseUnit();
        }

        #endregion // Parse interface

        #region Parse implementation

        string NextLine()
        {
            return m_lines[++m_currentLine];
        }

        class AsmFileBuilder
        {
            public List<AsmFunction> Functions { get; set; }
        }

        AsmUnit ParseUnit()
        {
            Dictionary<string, AsmFileBuilder> files = new Dictionary<string, AsmFileBuilder>();

            AsmFileBuilder fileBuilder = null;
            string line = CurrentLine;
            while (true) {
                if (line.StartsWith(FileStartID)) {
                    fileBuilder = ParseFileStart(files, line);
                } else if (line.Contains(FunctionStartID) && (fileBuilder != null)) {
                    fileBuilder.Functions.Add(ParseFunction());
                }

                if (IsEOF) {
                    break;
                }

                line = NextLine();
            }

            return ConvertFileBuildersToUnit(files);
        }

        AsmUnit ConvertFileBuildersToUnit(Dictionary<string, AsmFileBuilder> files)
        {
            AsmUnit unit = new AsmUnit {
                Files = new Dictionary<string, AsmFile>()
            };

            foreach (KeyValuePair<string, AsmFileBuilder> file in files) {
                AsmFileBuilder builder = file.Value;
                builder.Functions.Sort();
                unit.Files[file.Key] = new AsmFile {
                    Path = file.Key,
                    Functions = builder.Functions.ToArray()
                };
            }

            return unit;
        }

        AsmFileBuilder ParseFileStart(Dictionary<string, AsmFileBuilder> files, string line)
        {
            string path = line.Substring(FileStartID.Length + 1).ToLower();
            if (!files.TryGetValue(path, out AsmFileBuilder builder)) {
                builder = new AsmFileBuilder() {
                    Functions = new List<AsmFunction>()
                };
                files.Add(path, builder);
            }

            return builder;
        }

        AsmFunction ParseFunction()
        {
            AsmFunction function = ParseFunctionSignature(CurrentLine);

            NextLine();
            List<AsmBlock> blocks = new List<AsmBlock>();
            while (!CurrentLine.Contains(FunctionEndID)) {
                blocks.Add(ParseBlock());
            }

            function.Blocks = blocks.ToArray();
            Array.Sort(function.Blocks);

            function.Range = new LineRange(function.Blocks[0].Range.Min,
                function.Blocks[function.Blocks.Length - 1].Range.Max);
            return function;
        }

        AsmFunction ParseFunctionSignature(string signature)
        {
            // Format of the signature line:
            // mangled_name PROC (; demangled_name(, COMDAT))
            // Spaces may be any number of whitespace characters.

            int indexOfID = signature.IndexOf(FunctionStartID);
            string mangledName = signature.Substring(0, indexOfID).TrimEnd();

            string name = null;

            int nameStart = signature.IndexOf(';', indexOfID);
            if (nameStart != -1) {
                // The function signature may contain demangled name.
                ++nameStart;

                if (signature.EndsWith(FunctionCOMDAT)) {
                    name = signature.Substring(nameStart, signature.Length - nameStart - FunctionCOMDAT.Length);
                    name = name.Trim();
                    if (string.IsNullOrWhiteSpace(name)) {
                        name = mangledName;
                    } else if (name.EndsWith(",")) {
                        name = name.Substring(0, name.Length - 1);
                    }
                } else {
                    name = signature.Substring(nameStart);
                    name = name.Trim();
                }
            } else {
                // The function signature has only mangled name that is
                // probably just C name.
                name = mangledName;
            }

            return new AsmFunction {
                MangledName = mangledName,
                Name = name
            };
        }

        AsmBlock ParseBlock()
        {
            // Usual block of assembly starts with lines describing the
            // source file, followed by lines with assembly code:
            //
            // ; 12 {
            // ; 13     int value = ComputeValue();
            // mov rax, r11
            // add r2, r8
            //

            // However, some functions do not have any source code assigned
            // such as generated special functions, e.g., destructors.
            // Then only assembly code is present.

            SkipEmptyLines();

            if (CurrentLine[0] == ';') {
                return ParseBlockWithSource();
            } else {
                return ParseBlockAssembly();
            }
        }

        AsmBlock ParseBlockWithSource()
        {
            int minLine = ParseSourceLineNumber(CurrentLine);

            string lastSourceLine = null;
            while (CurrentLine[0] == ';') {
                lastSourceLine = CurrentLine;
                NextLine();
            }

            int maxLine = ParseSourceLineNumber(lastSourceLine);

            AsmBlock block = new AsmBlock {
                Range = new LineRange(minLine, maxLine)
            };

            SkipEmptyLines();

            Debug.Assert(CurrentLine[0] != ';');
            Debug.Assert(!CurrentLine.Contains(FunctionEndID));

            List<string> assembly = new List<string>();
            while (!IsEndOfBlock(CurrentLine)) {
                assembly.Add(CurrentLine);
                NextLine();
            }

            block.Assembly = assembly.ToArray();
            return block;
        }

        AsmBlock ParseBlockAssembly()
        {
            AsmBlock block = new AsmBlock {
                Range = LineRange.InvalidRange
            };

            List<string> assembly = new List<string>();
            while (!CurrentLine.Contains(FunctionEndID)) {
                assembly.Add(CurrentLine);
                NextLine();
            }

            block.Assembly = assembly.ToArray();
            return block;
        }

        bool IsEndOfBlock(string line)
        {
            return string.IsNullOrWhiteSpace(line) ||
                (line[0] == ';') ||
                line.Contains(FunctionEndID);
        }

        int ParseSourceLineNumber(string line)
        {
            const int NUMBER_START = 2;
            int numberEnd = line.IndexOf(' ', NUMBER_START);
            Debug.Assert(numberEnd != -1);
            return int.Parse(line.Substring(NUMBER_START, numberEnd - NUMBER_START));
        }

        void SkipEmptyLines()
        {
            while (string.IsNullOrWhiteSpace(CurrentLine)) {
                NextLine();
            }
        }

        #endregion // Parse implementation
    }
}
