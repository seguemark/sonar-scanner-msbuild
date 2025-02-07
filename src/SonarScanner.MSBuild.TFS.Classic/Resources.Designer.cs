﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SonarScanner.MSBuild.TFS.Classic {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SonarScanner.MSBuild.TFS.Classic.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CodeCoverage.exe found at {0}..
        /// </summary>
        internal static string CONV_DIAG_CodeCoverageFound {
            get {
                return ResourceManager.GetString("CONV_DIAG_CodeCoverageFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to VsTestToolsInstallerInstalledToolLocation environment variable doesn&apos;t contain full path to CodeCoverage.exe tool, seeking in standard place set by VSTestPlatformToolInstaller: {0} and {1}.
        /// </summary>
        internal static string CONV_DIAG_CodeCoverageIsNotInVariable {
            get {
                return ResourceManager.GetString("CONV_DIAG_CodeCoverageIsNotInVariable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Code coverage command line tool: {0}.
        /// </summary>
        internal static string CONV_DIAG_CommandLineToolInfo {
            get {
                return ResourceManager.GetString("CONV_DIAG_CommandLineToolInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attempting to locate the CodeCoverage.exe tool....
        /// </summary>
        internal static string CONV_DIAG_LocatingCodeCoverageTool {
            get {
                return ResourceManager.GetString("CONV_DIAG_LocatingCodeCoverageTool", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attempting to locate the CodeCoverage.exe tool using registry....
        /// </summary>
        internal static string CONV_DIAG_LocatingCodeCoverageToolRegistry {
            get {
                return ResourceManager.GetString("CONV_DIAG_LocatingCodeCoverageToolRegistry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attempting to locate the CodeCoverage.exe tool using setup configuration....
        /// </summary>
        internal static string CONV_DIAG_LocatingCodeCoverageToolSetupConfiguration {
            get {
                return ResourceManager.GetString("CONV_DIAG_LocatingCodeCoverageToolSetupConfiguration", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to VsTestToolsInstallerInstalledToolLocation environment variable detected, seeking for CodeCoverage.exe location....
        /// </summary>
        internal static string CONV_DIAG_LocatingCodeCoverageToolUserSuppliedProperty {
            get {
                return ResourceManager.GetString("CONV_DIAG_LocatingCodeCoverageToolUserSuppliedProperty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Multiple versions of VS are installed: {0}.
        /// </summary>
        internal static string CONV_DIAG_MultipleVsVersionsInstalled {
            get {
                return ResourceManager.GetString("CONV_DIAG_MultipleVsVersionsInstalled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Visual Studio setup configuration was not found..
        /// </summary>
        internal static string CONV_DIAG_SetupConfigurationNotSupported {
            get {
                return ResourceManager.GetString("CONV_DIAG_SetupConfigurationNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to convert the downloaded code coverage tool to XML. No code coverage information will be uploaded to SonarQube.
        ///Check that the downloaded code coverage file ({0}) is valid by opening it in Visual Studio. If it is not, check that the internet security settings on the build machine allow files to be downloaded from the Team Foundation Server machine..
        /// </summary>
        internal static string CONV_ERROR_ConversionToolFailed {
            get {
                return ResourceManager.GetString("CONV_ERROR_ConversionToolFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to convert the binary coverage file to XML. The expected output file was not found: {0}.
        /// </summary>
        internal static string CONV_ERROR_OutputFileNotFound {
            get {
                return ResourceManager.GetString("CONV_ERROR_OutputFileNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to find the code coverage command line tool. Possible cause: Visual Studio is not installed, or the installed version does not support code coverage..
        /// </summary>
        internal static string CONV_WARN_FailToFindConversionTool {
            get {
                return ResourceManager.GetString("CONV_WARN_FailToFindConversionTool", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CodeCoverage.exe was not found in the standard locations. Please provide the full path of the tool using the VsTestToolsInstallerInstalledToolLocation variable..
        /// </summary>
        internal static string CONV_WARN_UnableToFindCodeCoverageFileInUserSuppliedVariable {
            get {
                return ResourceManager.GetString("CONV_WARN_UnableToFindCodeCoverageFileInUserSuppliedVariable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Connected to {0}.
        /// </summary>
        internal static string DOWN_DIAG_ConnectedToTFS {
            get {
                return ResourceManager.GetString("DOWN_DIAG_ConnectedToTFS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Downloading coverage file from {0} to {1}.
        /// </summary>
        internal static string DOWN_DIAG_DownloadCoverageReportFromTo {
            get {
                return ResourceManager.GetString("DOWN_DIAG_DownloadCoverageReportFromTo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No code coverage reports were found for the current build..
        /// </summary>
        internal static string PROC_DIAG_NoCodeCoverageReportsFound {
            get {
                return ResourceManager.GetString("PROC_DIAG_NoCodeCoverageReportsFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to download the code coverage report..
        /// </summary>
        internal static string PROC_ERROR_FailedToDownloadReport {
            get {
                return ResourceManager.GetString("PROC_ERROR_FailedToDownloadReport", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &quot;Failed to download the code coverage report from {0}. The HTTP status code was {1} and the reason \&quot;{2}\&quot;&quot;.
        /// </summary>
        internal static string PROC_ERROR_FailedToDownloadReportReason {
            get {
                return ResourceManager.GetString("PROC_ERROR_FailedToDownloadReportReason", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to More than one code coverage result file was created. Only one report can be uploaded to SonarQube. Please modify the build definition so either {0} analysis is disabled or only one platform/flavor is built.
        /// </summary>
        internal static string PROC_ERROR_MultipleCodeCoverageReportsFound {
            get {
                return ResourceManager.GetString("PROC_ERROR_MultipleCodeCoverageReportsFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SonarQube Analysis Summary.
        /// </summary>
        internal static string SonarQubeSummarySectionHeader {
            get {
                return ResourceManager.GetString("SonarQubeSummarySectionHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Connecting to TFS....
        /// </summary>
        internal static string URL_DIAG_ConnectingToTfs {
            get {
                return ResourceManager.GetString("URL_DIAG_ConnectingToTfs", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Coverage Id: {0}, Platform {1}, Flavor {2}.
        /// </summary>
        internal static string URL_DIAG_CoverageReportInfo {
            get {
                return ResourceManager.GetString("URL_DIAG_CoverageReportInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Fetching build information....
        /// </summary>
        internal static string URL_DIAG_FetchingBuildInfo {
            get {
                return ResourceManager.GetString("URL_DIAG_FetchingBuildInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Fetch code coverage report info....
        /// </summary>
        internal static string URL_DIAG_FetchingCoverageReportInfo {
            get {
                return ResourceManager.GetString("URL_DIAG_FetchingCoverageReportInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ...done..
        /// </summary>
        internal static string URL_DIAG_Finished {
            get {
                return ResourceManager.GetString("URL_DIAG_Finished", resourceCulture);
            }
        }
    }
}
