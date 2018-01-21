using Microsoft.VisualStudio.VCProjectEngine;
using System;
using System.Linq;

namespace AsmExplorer
{
    class CLCommandLineBuilder
    {
        private VCCLCompilerTool m_CL;

        #region Construction

        public CLCommandLineBuilder(VCCLCompilerTool cl)
        {
            m_CL = cl;
        }

        #endregion // Construction

        #region Composition

        public static string Compose(params string[] options)
        {
            options = options.Where(option => (option != null)).ToArray();
            return string.Join(" ", options);
        }

        public string Composed
        {
            get
            {
                return Compose(AdditionalIncludeDirectories,
                    AdditionalOptions,
                    AdditionalUsingDirectories,
                    AssemblerListingLocation,
                    AssemblerOutput,
                    BasicRuntimeChecks,
                    BufferSecurityCheck,
                    CallingConvention,
                    CompileAs,
                    CompileOnly,
                    DefaultCharIsUnsigned,
                    DebugInformationFormat,
                    Detect64BitPortabilityProblems,
                    DisableLanguageExtensions,
                    DisableSpecificWarnings,
                    EnableEnhancedInstructionSet,
                    EnableFiberSafeOptimizations,
                    EnableFunctionLevelLinking,
                    EnableIntrinsicFunctions,
                    EnablePREfast,
                    ErrorReporting,
                    ExceptionHandling,
                    ExpandAttributedSource,
                    FavorSizeOrSpeed,
                    FloatingPointExceptions,
                    FloatingPointModel,
                    ForceConformanceInForLoopScope,
                    ForcedIncludeFiles,
                    ForcedUsingFiles,
                    FullIncludePath,
                    GeneratePreprocessedFile,
                    IgnoreStandardIncludePath,
                    InlineFunctionExpansion,
                    KeepComments,
                    MinimalRebuild,
                    OmitFramePointers,
                    ObjectFile,
                    OmitDefaultLibName,
                    OpenMP,
                    Optimization,
                    PrecompiledHeaderFile,
                    PreprocessorDefinitions,
                    ProgramDataBaseFileName,
                    RuntimeLibrary,
                    RuntimeTypeInfo,
                    ShowIncludes,
                    SmallerTypeCheck,
                    StringPooling,
                    SuppressStartupBanner,
                    StructMemberAlignment,
                    TreatWChar_tAsBuiltInType,
                    UndefineAllPreprocessorDefinitions,
                    UndefinePreprocessorDefinitions,
                    UseFullPaths,
                    UsePrecompiledHeader,
                    WarnAsError,
                    WarningLevel,
                    WholeProgramOptimization);
            }
        }

        #endregion // Composition

        #region String properties

        public string AdditionalIncludeDirectories
        {
            get { return CmdAdditionalIncludeDirectories(m_CL.AdditionalIncludeDirectories); }
        }

        public string AdditionalOptions
        {
            get { return CmdAdditionalOptions(m_CL.AdditionalOptions); }
        }

        public string AdditionalUsingDirectories
        {
            get { return CmdAdditionalUsingDirectories(m_CL.AdditionalUsingDirectories); }
        }

        public string AssemblerListingLocation
        {
            get { return CmdAssemblerListingLocation(m_CL.AssemblerListingLocation); }
        }

        public string AssemblerOutput
        {
            get { return CmdAssemblerOutput(m_CL.AssemblerOutput); }
        }

        public string BasicRuntimeChecks
        {
            get { return CmdBasicRuntimeChecks(m_CL.BasicRuntimeChecks); }
        }

        public string BufferSecurityCheck
        {
            get { return CmdBufferSecurityCheck(m_CL.BufferSecurityCheck); }
        }

        public string CallingConvention
        {
            get { return CmdCallingConvention(m_CL.CallingConvention); }
        }

        public string CompileAs
        {
            get { return CmdCompileAs(m_CL.CompileAs); }
        }

        public string CompileOnly
        {
            get { return CmdCompileOnly(m_CL.CompileOnly); }
        }

        public string DefaultCharIsUnsigned
        {
            get { return CmdDefaultCharIsUnsigned(m_CL.DefaultCharIsUnsigned); }
        }

        public string DebugInformationFormat
        {
            get { return CmdDebugInformationFormat(m_CL.DebugInformationFormat); }
        }

        public string Detect64BitPortabilityProblems
        {
            get { return CmdDetect64BitPortabilityProblems(m_CL.Detect64BitPortabilityProblems); }
        }

        public string DisableLanguageExtensions
        {
            get { return CmdDisableLanguageExtentions(m_CL.DisableLanguageExtensions); }
        }

        public string DisableSpecificWarnings
        {
            get { return CmdDisableSpecificWarnings(m_CL.DisableSpecificWarnings); }
        }

        public string EnableEnhancedInstructionSet
        {
            get { return CmdEnableEnhancedInstructionSet(m_CL.EnableEnhancedInstructionSet); }
        }

        public string EnableFiberSafeOptimizations
        {
            get { return CmdEnableFiberSafeOptimizations(m_CL.EnableFiberSafeOptimizations); }
        }

        public string EnableFunctionLevelLinking
        {
            get { return CmdEnableFunctionLevelLinking(m_CL.EnableFunctionLevelLinking); }
        }

        public string EnableIntrinsicFunctions
        {
            get { return CmdEnableIntrinsicFunctions(m_CL.EnableIntrinsicFunctions); }
        }

        public string EnablePREfast
        {
            get { return CmdEnablePREFast(m_CL.EnablePREfast); }
        }

        public string ErrorReporting
        {
            get { return CmdErrorReporting(m_CL.ErrorReporting); }
        }

        public string ExceptionHandling
        {
            get { return CmdExceptionHandling(m_CL.ExceptionHandling); }
        }

        public string ExpandAttributedSource
        {
            get { return CmdExpandAttributedSource(m_CL.ExpandAttributedSource); }
        }

        public string FavorSizeOrSpeed
        {
            get { return CmdFavorSizeOrSpeed(m_CL.FavorSizeOrSpeed); }
        }

        public string FloatingPointExceptions
        {
            get { return CmdFloatingPointExceptions(m_CL.FloatingPointExceptions); }
        }

        public string FloatingPointModel
        {
            get { return CmdFloatingPointModel(m_CL.floatingPointModel); }
        }

        public string ForceConformanceInForLoopScope
        {
            get { return CmdForceComformancceInLoopForScope(m_CL.ForceConformanceInForLoopScope); }
        }

        public string ForcedIncludeFiles
        {
            get { return CmdForcedIncludeFiles(m_CL.ForcedIncludeFiles); }
        }

        public string ForcedUsingFiles
        {
            get { return CmdForcedUsingFiles(m_CL.ForcedUsingFiles); }
        }

        public string FullIncludePath
        {
            get { return CmdFullIncludePath(m_CL.FullIncludePath); }
        }

        public string GeneratePreprocessedFile
        {
            get { return CmdGeneratePreprocessedFile(m_CL.GeneratePreprocessedFile); }
        }

        public string IgnoreStandardIncludePath
        {
            get { return CmdIgnoreStandardIncludePath(m_CL.IgnoreStandardIncludePath); }
        }

        public string InlineFunctionExpansion
        {
            get { return CmdInlineFunctionExpansion(m_CL.InlineFunctionExpansion); }
        }

        public string KeepComments
        {
            get { return CmdKeepComments(m_CL.KeepComments); }
        }

        public string MinimalRebuild
        {
            get { return CmdMinimalRebuild(m_CL.MinimalRebuild); }
        }

        public string OmitDefaultLibName
        {
            get { return CmdOmitDefaultLibName(m_CL.OmitDefaultLibName); }
        }

        public string OmitFramePointers
        {
            get { return CmdOmitFramePointers(m_CL.OmitFramePointers); }
        }

        public string ObjectFile
        {
            get { return CmdObjectFile(m_CL.ObjectFile); }
        }

        public string OpenMP
        {
            get { return CmdOpenMP(m_CL.OpenMP); }
        }

        public string Optimization
        {
            get { return CmdOptimization(m_CL.Optimization); }
        }

        public string PrecompiledHeaderFile
        {
            get { return CmdPrecompiledHeaderFile(m_CL.PrecompiledHeaderFile); }
        }

        public string PreprocessorDefinitions
        {
            get { return CmdPreprocessorDefinitions(m_CL.PreprocessorDefinitions); }
        }

        public string ProgramDataBaseFileName
        {
            get { return CmdProgramDatabaseFileName(m_CL.ProgramDataBaseFileName); }
        }

        public string RuntimeLibrary
        {
            get { return CmdRuntimeLibrary(m_CL.RuntimeLibrary); }
        }

        public string RuntimeTypeInfo
        {
            get { return CmdRuntimeTypeInfo(m_CL.RuntimeTypeInfo); }
        }

        public string ShowIncludes
        {
            get { return CmdShowIncludes(m_CL.ShowIncludes); }
        }

        public string SmallerTypeCheck
        {
            get { return CmdSmallerTypeCheck(m_CL.SmallerTypeCheck); }
        }

        public string StringPooling
        {
            get { return CmdStringPooling(m_CL.StringPooling); }
        }

        public string SuppressStartupBanner
        {
            get { return CmdSupressStartupBanner(m_CL.SuppressStartupBanner); }
        }

        public string StructMemberAlignment
        {
            get { return CmdStructMemberAlignment(m_CL.StructMemberAlignment); }
        }

        public string TreatWChar_tAsBuiltInType
        {
            get { return CmdTreatWChar_tAsBuiltInType(m_CL.TreatWChar_tAsBuiltInType); }
        }

        public string UndefineAllPreprocessorDefinitions
        {
            get { return CmdUndefineAllPreprocessorDefinitions(m_CL.UndefineAllPreprocessorDefinitions); }
        }

        public string UndefinePreprocessorDefinitions
        {
            get { return CmdUndefinePreprocessorDefinitions(m_CL.UndefinePreprocessorDefinitions); }
        }

        public string UseFullPaths
        {
            get { return CmdUseFullPaths(m_CL.UseFullPaths); }
        }

        public string UsePrecompiledHeader
        {
            get { return CmdUsePrecompilerHeader(m_CL.UsePrecompiledHeader, m_CL.PrecompiledHeaderThrough); }
        }

        public string WarnAsError
        {
            get { return CmdWarnAsError(m_CL.WarnAsError); }
        }

        public string WarningLevel
        {
            get { return CmdWarningLevel(m_CL.WarningLevel); }
        }

        public string WholeProgramOptimization
        {
            get { return CmdWholeProgramOptimization(m_CL.WholeProgramOptimization); }
        }

        #endregion // String properties

        #region Conversion methods

        private static string CmdAdditionalIncludeDirectories(string directories)
        {
            return CmdSemicolonSeparated("/I", directories);
        }

        public static string CmdAdditionalOptions(string options)
        {
            return options;
        }

        public static string CmdAdditionalUsingDirectories(string directories)
        {
            return CmdSemicolonSeparated("/AI", directories);
        }

        public static string CmdAssemblerListingLocation(string location)
        {
            return "/Fa" + SurroundWithQuotes(location);
        }

        public static string CmdAssemblerOutput(asmListingOption option)
        {
            switch (option)
            {
                case asmListingOption.asmListingAssemblyOnly:
                    return "/FA";
                case asmListingOption.asmListingAsmSrc:
                    return "/FAs";
                case asmListingOption.asmListingAsmMachine:
                    return "/FAc";
                case asmListingOption.asmListingAsmMachineSrc:
                    return "/FAu";
            }
            return null;
        }

        public static string CmdBasicRuntimeChecks(basicRuntimeCheckOption option)
        {
            switch (option)
            {
                case basicRuntimeCheckOption.runtimeCheckUninitVariables:
                    return "/RTCu";
                case basicRuntimeCheckOption.runtimeCheckStackFrame:
                    return "/RTCs";
                case basicRuntimeCheckOption.runtimeBasicCheckAll:
                    return "/RTCsu";

            }
            return null;
        }

        public static string CmdBufferSecurityCheck(bool enabled)
        {
            return CmdBoolean(enabled, "/GS");
        }

        public static string CmdCallingConvention(callingConventionOption option)
        {
            switch (option)
            {
                case callingConventionOption.callConventionCDecl:
                    return "/Gd";
                case callingConventionOption.callConventionFastCall:
                    return "/Gr";
                case callingConventionOption.callConventionStdCall:
                    return "/Gz";
            }
            return null;
        }

        public static string CmdCompileAs(CompileAsOptions option)
        {
            switch (option)
            {
                case CompileAsOptions.compileAsC:
                    return "/TC";
                case CompileAsOptions.compileAsCPlusPlus:
                    return "/TP";
            }
            return null;
        }

        public static string CmdCompileOnly(bool enabled)
        {
            return CmdBoolean(enabled, "/c");
        }

        public static string CmdDebugInformationFormat(debugOption option)
        {
            switch (option)
            {
                case debugOption.debugOldStyleInfo:
                    return "/Z7";
                case debugOption.debugEnabled:
                    return "/Zi";
                case debugOption.debugEditAndContinue:
                    return "/ZI";
            }
            return null;
        }

        public static string CmdDefaultCharIsUnsigned(bool isUnsigned)
        {
            return CmdBoolean(isUnsigned, "/J");
        }

        public static string CmdDetect64BitPortabilityProblems(bool enabled)
        {
            return CmdBoolean(enabled, "/Wp64");
        }

        public static string CmdDisableLanguageExtentions(bool disableExtensions)
        {
            return CmdBoolean(disableExtensions, "/Ze");
        }

        public static string CmdDisableSpecificWarnings(string warnings)
        {
            return CmdSemicolonSeparated("/wd", warnings);
        }

        public static string CmdEnableEnhancedInstructionSet(enhancedInstructionSetType setType)
        {
            switch (setType)
            {
                case enhancedInstructionSetType.enhancedInstructionSetTypeSIMD:
                    return "/arch:SIMD";
                case enhancedInstructionSetType.enhancedInstructionSetTypeSIMD2:
                    return "/arch:SIMD2";
            }
            return null;
        }

        public static string CmdEnableFiberSafeOptimizations(bool enabled)
        {
            return CmdBoolean(enabled, "/GT");
        }

        public static string CmdEnableFunctionLevelLinking(bool enabled)
        {
            return CmdBoolean(enabled, "/Gy");
        }

        public static string CmdEnableIntrinsicFunctions(bool enabled)
        {
            return CmdBoolean(enabled, "/Oi");
        }

        public static string CmdEnablePREFast(bool enabled)
        {
            return CmdBoolean(enabled, "/analyze");
        }

        public static string CmdErrorReporting(compilerErrorReportingType type)
        {
            switch (type)
            {
                case compilerErrorReportingType.compilerErrorReportingPrompt:
                    return "/errorreport:prompt";
                case compilerErrorReportingType.compilerErrorReportingQueue:
                    return "/errorreport:queue";
            }
            return null;
        }

        public static string CmdExceptionHandling(cppExceptionHandling handling)
        {
            switch (handling)
            {
                case cppExceptionHandling.cppExceptionHandlingNo:
                    return "/EHc-s-";
                case cppExceptionHandling.cppExceptionHandlingYes:
                    return "/EHcs";
                case cppExceptionHandling.cppExceptionHandlingYesWithSEH:
                    return "/EHa";
            }
            return null;
        }

        public static string CmdExpandAttributedSource(bool enabled)
        {
            return CmdBoolean(enabled, "/Fx");
        }

        public static string CmdFavorSizeOrSpeed(favorSizeOrSpeedOption option)
        {
            switch (option)
            {
                case favorSizeOrSpeedOption.favorSize:
                    return "/Os";
                case favorSizeOrSpeedOption.favorSpeed:
                    return "/Ot";
            }
            return null;
        }

        public static string CmdFloatingPointExceptions(bool enabled)
        {
            return CmdBoolean(enabled, "/fp:except", "/fp:except-");
        }

        public static string CmdFloatingPointModel(floatingPointModel model)
        {
            switch (model)
            {
                case floatingPointModel.FloatingPointFast:
                    return "/fp:fase";
                case floatingPointModel.FloatingPointPrecise:
                    return "/fp:precise";
                case floatingPointModel.FloatingPointStrict:
                    return "/fp:strict";
            }
            return null;
        }

        public static string CmdForceComformancceInLoopForScope(bool enabled)
        {
            return CmdBoolean(enabled, "/Zc:forScope");
        }

        public static string CmdForcedIncludeFiles(string files)
        {
            return CmdSemicolonSeparated("/FI", files);
        }

        public static string CmdForcedUsingFiles(string files)
        {
            return CmdSemicolonSeparated("/FU", files);
        }

        public static string CmdFullIncludePath(string paths)
        {
            return CmdSemicolonSeparated("/I", paths);
        }

        public static string CmdGeneratePreprocessedFile(preprocessOption option)
        {
            switch (option)
            {
                case preprocessOption.preprocessNoLineNumbers:
                    return "/EP /P";
                case preprocessOption.preprocessYes:
                    return "/P";
            }
            return null;
        }

        public static string CmdGenerateXMLDocumentation(bool enabled)
        {
            return CmdBoolean(enabled, "/doc");
        }

        public static string CmdIgnoreStandardIncludePath(bool enabled)
        {
            return CmdBoolean(enabled, "/X");
        }

        public static string CmdInlineFunctionExpansion(inlineExpansionOption option)
        {
            switch (option)
            {
                case inlineExpansionOption.expandDisable:
                    return "/Ob0";
                case inlineExpansionOption.expandOnlyInline:
                    return "/Ob1";
                case inlineExpansionOption.expandAnySuitable:
                    return "/Ob2";
            }
            return null;
        }

        public static string CmdKeepComments(bool enabled)
        {
            return CmdBoolean(enabled, "/C");
        }

        public static string CmdMinimalRebuild(bool enabled)
        {
            return CmdBoolean(enabled, "/Gm");
        }

        public static string CmdObjectFile(string file)
        {
            return "/Fo" + file;
        }

        public static string CmdOmitDefaultLibName(bool enabled)
        {
            return CmdBoolean(enabled, "/Zl");
        }

        public static string CmdOmitFramePointers(bool enabled)
        {
            return CmdBoolean(enabled, "/Oy");
        }

        public static string CmdOpenMP(bool enabled)
        {
            return CmdBoolean(enabled, "/openmp", "/openmp-");
        }

        public static string CmdOptimization(optimizeOption option)
        {
            switch (option)
            {
                case optimizeOption.optimizeDisabled:
                    return "/Od";
                case optimizeOption.optimizeMinSpace:
                    return "/O1";
                case optimizeOption.optimizeMaxSpeed:
                    return "/O2";
                case optimizeOption.optimizeFull:
                    return "/Ox";
            }

            return null;
        }

        public static string CmdPrecompiledHeaderFile(string pch)
        {
            return "/Fp" + SurroundWithQuotes(pch);
        }

        public static string CmdPreprocessorDefinitions(string definitions)
        {
            return CmdSemicolonSeparated("/D", definitions);
        }

        public static string CmdProgramDatabaseFileName(string filename)
        {
            return "/Fd" + SurroundWithQuotes(filename);
        }

        public static string CmdRuntimeLibrary(runtimeLibraryOption option)
        {
            switch (option)
            {
                case runtimeLibraryOption.rtMultiThreadedDebug:
                    return "/MTd";
                case runtimeLibraryOption.rtMultiThreaded:
                    return "/MT";
                case runtimeLibraryOption.rtMultiThreadedDebugDLL:
                    return "/MDd";
                case runtimeLibraryOption.rtMultiThreadedDLL:
                    return "/MDd";
            }
            return null;
        }

        public static string CmdRuntimeTypeInfo(bool enabled)
        {
            if (enabled)
                return "/GR";
            else
                return "/GR-";
        }

        public static string CmdShowIncludes(bool enabled)
        {
            return CmdBoolean(enabled, "/showinclude");
        }

        public static string CmdSmallerTypeCheck(bool enabled)
        {
            return CmdBoolean(enabled, "/RTC");
        }

        public static string CmdStringPooling(bool enabled)
        {
            return CmdBoolean(enabled, "/GF");
        }

        public static string CmdStructMemberAlignment(structMemberAlignOption option)
        {
            switch (option)
            {
                case structMemberAlignOption.alignSingleByte:
                    return "/Zp1";
                case structMemberAlignOption.alignTwoBytes:
                    return "/Zp2";
                case structMemberAlignOption.alignFourBytes:
                    return "/Zp4";
                case structMemberAlignOption.alignEightBytes:
                    return "/Zp8";
                case structMemberAlignOption.alignSixteenBytes:
                    return "/Zp16";
            }
            return null;
        }

        public static string CmdSupressStartupBanner(bool enabled)
        {
            return CmdBoolean(enabled, "/nologo");
        }

        public static string CmdTreatWChar_tAsBuiltInType(bool enabled)
        {
            return CmdBoolean(enabled, "/Zc:wchar_t");
        }

        public static string CmdUndefineAllPreprocessorDefinitions(bool enabled)
        {
            return CmdBoolean(enabled, "/U");
        }

        public static string CmdUndefinePreprocessorDefinitions(string definitions)
        {
            return CmdSemicolonSeparated("/u", definitions);
        }

        public static string CmdUseFullPaths(bool enabled)
        {
            return CmdBoolean(enabled, "/FC");
        }

        public static string CmdUsePrecompilerHeader(pchOption option, string filename)
        {
            switch (option)
            {
                case pchOption.pchCreateUsingSpecific:
                    return "/Yc" + SurroundWithQuotes(filename);
                case pchOption.pchUseUsingSpecific:
                    return "/Yu" + SurroundWithQuotes(filename);
            }
            return null;
        }

        public static string CmdWarnAsError(bool enabled)
        {
            return CmdBoolean(enabled, "/WX");
        }

        public static string CmdWarningLevel(warningLevelOption option)
        {
            switch (option)
            {
                case warningLevelOption.warningLevel_0:
                    return "/W0";
                case warningLevelOption.warningLevel_1:
                    return "/W1";
                case warningLevelOption.warningLevel_2:
                    return "/W2";
                case warningLevelOption.warningLevel_3:
                    return "/W3";
                case warningLevelOption.warningLevel_4:
                    return "/W4";
            }
            return null;
        }

        public static string CmdWholeProgramOptimization(bool enabled)
        {
            return CmdBoolean(enabled, "/GL");
        }

        private static string CmdSemicolonSeparated(string option, string semicolonSeparated)
        {
            string cmd = null;

            if (!string.IsNullOrEmpty(semicolonSeparated))
            {
                string[] values = semicolonSeparated.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                string[] composed = new string[values.Length];

                for (int i = 0; i < values.Length; ++i)
                {
                    composed[i] = option + SurroundWithQuotes(values[i]);
                }

                cmd = string.Join(" ", composed);
            }

            return cmd;
        }

        public static string SurroundWithQuotes(string s)
        {
            return SurroundWith(s, "\"");
        }

        private static string SurroundWith(string s, string surround)
        {
            return surround + s + surround;
        }

        private static string CmdBoolean(bool enabled, string enabledOption, string disabledOption = null)
        {
            if (enabled)
                return enabledOption;
            else
                return disabledOption;
        }

        #endregion // Conversion methods
    }
}
