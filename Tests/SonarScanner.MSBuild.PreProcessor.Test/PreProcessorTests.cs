﻿/*
 * SonarScanner for .NET
 * Copyright (C) 2016-2022 SonarSource SA
 * mailto: info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarScanner.MSBuild.Common;
using SonarScanner.MSBuild.Common.Interfaces;
using SonarScanner.MSBuild.PreProcessor.Roslyn.Model;
using TestUtilities;

namespace SonarScanner.MSBuild.PreProcessor.Test
{
    [TestClass]
    public class PreProcessorTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Constructor_NullArguments()
        {
            var logger = Mock.Of<ILogger>();
            var factory = Mock.Of<IPreprocessorObjectFactory>();
            ((Func<PreProcessor>)(() => new PreProcessor(null, logger))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("factory");
            ((Func<PreProcessor>)(() => new PreProcessor(factory, null))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        [TestMethod]
        public void PreProc_InvalidArgs()
        {
            var factory = new MockObjectFactory();
            var preProcessor = new PreProcessor(factory, factory.Logger);

            preProcessor.Invoking(async x => await x.Execute(null)).Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task PreProc_InvalidCommandLineArgs()
        {
            var factory = new MockObjectFactory();
            var sut = new PreProcessor(factory, factory.Logger);

            (await sut.Execute(new[] { "invalid args" })).Should().Be(false);
            factory.Logger.AssertErrorLogged(
@"Expecting at least the following command line argument:
- SonarQube/SonarCloud project key
The full path to a settings file can also be supplied. If it is not supplied, the exe will attempt to locate a default settings file in the same directory as the SonarQube Scanner for MSBuild.
Use '/?' or '/h' to see the help message.");
        }

        [TestMethod]
        public async Task PreProc_CannotCreateDirectories()
        {
            using var scope = new TestScope(TestContext);
            var factory = new MockObjectFactory();
            var preProcessor = new PreProcessor(factory, factory.Logger);
            var configDirectory = Path.Combine(scope.WorkingDir, "conf");
            Directory.CreateDirectory(configDirectory);
            using var lockedFile = new FileStream(Path.Combine(configDirectory, "LockedFile.txt"), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);

            (await preProcessor.Execute(CreateArgs())).Should().BeFalse();
            factory.Logger.AssertErrorLogged(
@$"Failed to create an empty directory '{configDirectory}'. Please check that there are no open or read-only files in the directory and that you have the necessary read/write permissions.
  Detailed error message: The process cannot access the file 'LockedFile.txt' because it is being used by another process.");
        }

        [TestMethod]
        public async Task PreProc_License_Invalid()
        {
            using var scope = new TestScope(TestContext);
            var factory = new MockObjectFactory();
            var preProcessor = new PreProcessor(factory, factory.Logger);
            factory.Server.IsServerLicenseValidImplementation = () => Task.FromResult(false);

            (await preProcessor.Execute(CreateArgs())).Should().BeFalse();
            factory.Logger.AssertErrorLogged("Your SonarQube instance seems to have an invalid license. Please check it. Server url: http://host");
        }

        [TestMethod]
        public async Task PreProc_License_Throws()
        {
            using var scope = new TestScope(TestContext);
            var factory = new MockObjectFactory();
            var preProcessor = new PreProcessor(factory, factory.Logger);
            factory.Server.IsServerLicenseValidImplementation = () => throw new InvalidOperationException("Some error was thrown during license check.");

            (await preProcessor.Execute(CreateArgs())).Should().BeFalse();
            factory.Logger.AssertErrorLogged("Some error was thrown during license check.");
        }

        [TestMethod]
        public async Task PreProc_TargetsNotInstalled()
        {
            using var scope = new TestScope(TestContext);
            var factory = new MockObjectFactory();
            var preProcessor = new PreProcessor(factory, factory.Logger);

            (await preProcessor.Execute(CreateArgs().Append("/install:false"))).Should().BeTrue();
            factory.Logger.AssertDebugLogged("Skipping installing the ImportsBefore targets file.");
        }

        [TestMethod]
        public async Task PreProc_FetchArgumentsAndRuleSets_ConnectionIssue()
        {
            using var scope = new TestScope(TestContext);
            var factory = new MockObjectFactory();
            var preProcessor = new PreProcessor(factory, factory.Logger);
            factory.Server.TryGetQualityProfilePreprocessing = () => throw new WebException("Could not connect to remote server", WebExceptionStatus.ConnectFailure);

            (await preProcessor.Execute(CreateArgs())).Should().BeFalse();
            factory.Logger.AssertErrorLogged("Could not connect to the SonarQube server. Check that the URL is correct and that the server is available. URL: http://host");
        }

        [TestMethod]
        public async Task PreProc_FetchArgumentsAndRuleSets_ServerReturnsUnexpectedStatus()
        {
            using var scope = new TestScope(TestContext);
            var factory = new MockObjectFactory();
            var preProcessor = new PreProcessor(factory, factory.Logger);
            factory.Server.TryGetQualityProfilePreprocessing = () => throw new WebException("Something else went wrong");

            await preProcessor.Invoking(async x => await x.Execute(CreateArgs())).Should().ThrowAsync<WebException>().WithMessage("Something else went wrong");
        }

        [TestMethod]
        public async Task PreProc_EndToEnd_SuccessCase()
        {
            // Checks end-to-end happy path for the pre-processor i.e.
            // * arguments are parsed
            // * targets are installed
            // * server properties are fetched
            // * rule sets are generated
            // * config file is created
            using var scope = new TestScope(TestContext);
            var factory = new MockObjectFactory();
            factory.Server.Data.SonarQubeVersion = new Version(1, 2, 3, 4);
            var settings = factory.ReadSettings();
            var preProcessor = new PreProcessor(factory, factory.Logger);

            var success = await preProcessor.Execute(CreateArgs());
            success.Should().BeTrue("Expecting the pre-processing to complete successfully");

            AssertDirectoriesCreated(settings);

            factory.TargetsInstaller.Verify(x => x.InstallLoaderTargets(scope.WorkingDir), Times.Once());
            factory.Server.AssertMethodCalled("GetProperties", 1);
            factory.Server.AssertMethodCalled("GetAllLanguages", 1);
            factory.Server.AssertMethodCalled("TryGetQualityProfile", 2); // C# and VBNet
            factory.Server.AssertMethodCalled("GetRules", 2); // C# and VBNet

            factory.Logger.AssertDebugLogged("Base branch parameter was not provided. Incremental PR analysis is disabled.");
            factory.Logger.AssertDebugLogged("Processing analysis cache");

            var config = AssertAnalysisConfig(settings.AnalysisConfigFilePath, 2, factory.Logger);
            config.SonarQubeVersion.Should().Be("1.2.3.4");
            config.GetConfigValue(SonarProperties.PullRequestCacheBasePath, null).Should().Be(Path.GetDirectoryName(scope.WorkingDir));
        }

        [TestMethod]
        public async Task PreProc_WithPullRequestBranch()
        {
            using var scope = new TestScope(TestContext);
            var factory = new MockObjectFactory();
            factory.Server.Data.Languages.Add("cs");
            var preProcessor = new PreProcessor(factory, factory.Logger);

            var args = CreateArgs(properties: new Dictionary<string, string> { { SonarProperties.PullRequestBase, "BASE_BRANCH" } });
            var success = await preProcessor.Execute(args);
            success.Should().BeTrue("Expecting the pre-processing to complete successfully");

            factory.Logger.InfoMessages.Should().Contain("Processing pull request with base branch 'BASE_BRANCH'.");
        }

        [TestMethod]
        public async Task PreProc_EndToEnd_SuccessCase_NoActiveRule()
        {
            using var scope = new TestScope(TestContext);
            var factory = new MockObjectFactory();
            factory.Server.Data.FindProfile("qp1").Rules.Clear();
            var settings = factory.ReadSettings();
            var preProcessor = new PreProcessor(factory, factory.Logger);

            var success = await preProcessor.Execute(CreateArgs());
            success.Should().BeTrue("Expecting the pre-processing to complete successfully");

            AssertDirectoriesCreated(settings);

            factory.TargetsInstaller.Verify(x => x.InstallLoaderTargets(scope.WorkingDir), Times.Once());
            factory.Server.AssertMethodCalled("GetProperties", 1);
            factory.Server.AssertMethodCalled("GetAllLanguages", 1);
            factory.Server.AssertMethodCalled("TryGetQualityProfile", 2); // C# and VBNet
            factory.Server.AssertMethodCalled("GetRules", 2); // C# and VBNet

            AssertAnalysisConfig(settings.AnalysisConfigFilePath, 2, factory.Logger);
        }

        [TestMethod]
        public async Task PreProc_EndToEnd_SuccessCase_With_Organization()
        {
            // Checks end-to-end happy path for the pre-processor i.e.
            // * arguments are parsed
            // * targets are installed
            // * server properties are fetched
            // * rule sets are generated
            // * config file is created
            using var scope = new TestScope(TestContext);
            var factory = new MockObjectFactory(organization: "organization");
            var settings = factory.ReadSettings();
            var preProcessor = new PreProcessor(factory, factory.Logger);

            var success = await preProcessor.Execute(CreateArgs("organization"));
            success.Should().BeTrue("Expecting the pre-processing to complete successfully");

            AssertDirectoriesCreated(settings);

            factory.TargetsInstaller.Verify(x => x.InstallLoaderTargets(scope.WorkingDir), Times.Once());
            factory.Server.AssertMethodCalled("GetProperties", 1);
            factory.Server.AssertMethodCalled("GetAllLanguages", 1);
            factory.Server.AssertMethodCalled("TryGetQualityProfile", 2); // C# and VBNet
            factory.Server.AssertMethodCalled("GetRules", 2); // C# and VBNet

            AssertAnalysisConfig(settings.AnalysisConfigFilePath, 2, factory.Logger);
        }

        [DataTestMethod]
        [DataRow("6.7.0.22152", true)]
        [DataRow("8.8.0.1121", false)]
        public async Task PreProc_EndToEnd_ShouldWarnOrNot_SonarQubeDeprecatedVersion(string sqVersion, bool shouldWarn)
        {
            using var scope = new TestScope(TestContext);
            var factory = new MockObjectFactory();
            factory.Server.Data.SonarQubeVersion = new Version(sqVersion);
            var preProcessor = new PreProcessor(factory, factory.Logger);

            var success = await preProcessor.Execute(CreateArgs());
            success.Should().BeTrue("Expecting the pre-processing to complete successfully");

            factory.TargetsInstaller.Verify(x => x.InstallLoaderTargets(scope.WorkingDir), Times.Once());

            if (shouldWarn)
            {
                factory.Server.AssertWarningWritten("version is below supported");
            }
            else
            {
                factory.Server.AssertNoWarningWritten();
            }
        }

        [TestMethod]
        public async Task PreProc_NoPlugin()
        {
            using var scope = new TestScope(TestContext);
            var factory = new MockObjectFactory();
            factory.Server.Data.Languages.Clear();
            factory.Server.Data.Languages.Add("invalid_plugin");
            var settings = factory.ReadSettings();
            var preProcessor = new PreProcessor(factory, factory.Logger);

            var success = await preProcessor.Execute(CreateArgs());
            success.Should().BeTrue("Expecting the pre-processing to complete successfully");

            AssertDirectoriesCreated(settings);

            factory.TargetsInstaller.Verify(x => x.InstallLoaderTargets(scope.WorkingDir), Times.Once());
            factory.Server.AssertMethodCalled("GetProperties", 1);
            factory.Server.AssertMethodCalled("GetAllLanguages", 1);
            factory.Server.AssertMethodCalled("TryGetQualityProfile", 0);   // No valid plugin
            factory.Server.AssertMethodCalled("GetRules", 0);               // No valid plugin

            AssertAnalysisConfig(settings.AnalysisConfigFilePath, 0, factory.Logger);

            // only contains SonarQubeAnalysisConfig (no rulesets or additional files)
            AssertDirectoryContains(settings.SonarConfigDirectory, Path.GetFileName(settings.AnalysisConfigFilePath));
        }

        [TestMethod]
        public async Task PreProc_NoProject()
        {
            using var scope = new TestScope(TestContext);
            var factory = new MockObjectFactory(false);
            factory.Server.Data
                .AddQualityProfile("qp1", "cs", null)
                .AddProject("invalid")
                .AddRule(new SonarRule("fxcop", "cs.rule1"))
                .AddRule(new SonarRule("fxcop", "cs.rule2"));
            factory.Server.Data
                .AddQualityProfile("qp2", "vbnet", null)
                .AddProject("invalid")
                .AddRule(new SonarRule("fxcop-vbnet", "vb.rule1"))
                .AddRule(new SonarRule("fxcop-vbnet", "vb.rule2"));
            var settings = factory.ReadSettings();
            var preProcessor = new PreProcessor(factory, factory.Logger);

            var success = await preProcessor.Execute(CreateArgs());
            success.Should().BeTrue("Expecting the pre-processing to complete successfully");

            AssertDirectoriesCreated(settings);

            factory.TargetsInstaller.Verify(x => x.InstallLoaderTargets(scope.WorkingDir), Times.Once());
            factory.Server.AssertMethodCalled("GetProperties", 1);
            factory.Server.AssertMethodCalled("GetAllLanguages", 1);
            factory.Server.AssertMethodCalled("TryGetQualityProfile", 2); // C# and VBNet
            factory.Server.AssertMethodCalled("GetRules", 0); // no quality profile assigned to project

            AssertAnalysisConfig(settings.AnalysisConfigFilePath, 0, factory.Logger);

            // only contains SonarQubeAnalysisConfig (no rulesets or additional files)
            AssertDirectoryContains(settings.SonarConfigDirectory, Path.GetFileName(settings.AnalysisConfigFilePath));
        }

        [TestMethod]
        public async Task PreProc_HandleAnalysisException()
        {
            // Checks end-to-end behavior when AnalysisException is thrown inside FetchArgumentsAndRulesets
            using var scope = new TestScope(TestContext);
            var factory = new MockObjectFactory();
            var exceptionWasThrown = false;
            factory.Server.TryGetQualityProfilePreprocessing = () =>
            {
                exceptionWasThrown = true;
                throw new AnalysisException("This message and stacktrace should not propagate to the users");
            };
            var preProcessor = new PreProcessor(factory, factory.Logger);
            var success = await preProcessor.Execute(CreateArgs("InvalidOrganization"));    // Should not throw
            success.Should().BeFalse("Expecting the pre-processing to fail");
            exceptionWasThrown.Should().BeTrue();
        }

        [TestMethod]
        // Regression test for https://github.com/SonarSource/sonar-scanner-msbuild/issues/699
        public async Task PreProc_EndToEnd_Success_LocalSettingsAreUsedInSonarLintXML()
        {
            // Checks that local settings are used when creating the SonarLint.xml file, overriding
            using var scope = new TestScope(TestContext);
            var factory = new MockObjectFactory();
            factory.Server.Data.ServerProperties.Add("shared.key1", "server shared value 1");
            factory.Server.Data.ServerProperties.Add("shared.CASING", "server upper case value");
            // Local settings that should override matching server settings
            var args = new List<string>(CreateArgs())
            {
                "/d:local.key=local value 1",
                "/d:shared.key1=local shared value 1 - should override server value",
                "/d:shared.casing=local lower case value"
            };
            var settings = factory.ReadSettings();
            var preProcessor = new PreProcessor(factory, factory.Logger);

            var success = await preProcessor.Execute(args);
            success.Should().BeTrue("Expecting the pre-processing to complete successfully");

            // Check the settings used when creating the SonarLint file - local and server settings should be merged
            factory.AnalyzerProvider.SuppliedSonarProperties.Should().NotBeNull();
            factory.AnalyzerProvider.SuppliedSonarProperties.AssertExpectedPropertyValue("server.key", "server value 1");
            factory.AnalyzerProvider.SuppliedSonarProperties.AssertExpectedPropertyValue("local.key", "local value 1");
            factory.AnalyzerProvider.SuppliedSonarProperties.AssertExpectedPropertyValue("shared.key1", "local shared value 1 - should override server value");
            // Keys are case-sensitive so differently cased values should be preserved
            factory.AnalyzerProvider.SuppliedSonarProperties.AssertExpectedPropertyValue("shared.CASING", "server upper case value");
            factory.AnalyzerProvider.SuppliedSonarProperties.AssertExpectedPropertyValue("shared.casing", "local lower case value");

            // Check the settings used when creating the config file - settings should be separate
            var actualConfig = AssertAnalysisConfig(settings.AnalysisConfigFilePath, 2, factory.Logger);
            AssertExpectedLocalSetting(actualConfig, "local.key", "local value 1");
            AssertExpectedLocalSetting(actualConfig, "shared.key1", "local shared value 1 - should override server value");
            AssertExpectedLocalSetting(actualConfig, "shared.casing", "local lower case value");

            AssertExpectedServerSetting(actualConfig, "server.key", "server value 1");
            AssertExpectedServerSetting(actualConfig, "shared.key1", "server shared value 1");
            AssertExpectedServerSetting(actualConfig, "shared.CASING", "server upper case value");
        }

        private static IEnumerable<string> CreateArgs(string organization = null, Dictionary<string, string> properties = null)
        {
            yield return "/k:key";
            yield return "/n:name";
            yield return "/v:1.0";
            if (organization != null)
            {
                yield return $"/o:{organization}";
            }
            yield return "/d:cmd.line1=cmdline.value.1";
            yield return "/d:sonar.host.url=http://host";
            yield return "/d:sonar.log.level=INFO|DEBUG";

            if (properties != null)
            {
                foreach (var pair in properties)
                {
                    yield return $"/d:{pair.Key}={pair.Value}";
                }
            }
        }

        private static void AssertDirectoriesCreated(IBuildSettings settings)
        {
            AssertDirectoryExists(settings.AnalysisBaseDirectory);
            AssertDirectoryExists(settings.SonarConfigDirectory);
            AssertDirectoryExists(settings.SonarOutputDirectory);
            // The bootstrapper is responsible for creating the bin directory
        }

        private AnalysisConfig AssertAnalysisConfig(string filePath, int noAnalyzers, TestLogger logger)
        {
            logger.AssertErrorsLogged(0);
            logger.AssertWarningsLogged(0);
            logger.AssertVerbosity(LoggerVerbosity.Debug);

            AssertConfigFileExists(filePath);
            var actualConfig = AnalysisConfig.Load(filePath);
            actualConfig.SonarProjectKey.Should().Be("key", "Unexpected project key");
            actualConfig.SonarProjectName.Should().Be("name", "Unexpected project name");
            actualConfig.SonarProjectVersion.Should().Be("1.0", "Unexpected project version");
            actualConfig.AnalyzersSettings.Should().NotBeNull("Analyzer settings should not be null");
            actualConfig.AnalyzersSettings.Should().HaveCount(noAnalyzers);

            AssertExpectedLocalSetting(actualConfig, SonarProperties.HostUrl, "http://host");
            AssertExpectedLocalSetting(actualConfig, "cmd.line1", "cmdline.value.1");
            AssertExpectedServerSetting(actualConfig, "server.key", "server value 1");

            return actualConfig;
        }

        private void AssertConfigFileExists(string filePath)
        {
            File.Exists(filePath).Should().BeTrue("Expecting the analysis config file to exist. Path: {0}", filePath);
            TestContext.AddResultFile(filePath);
        }

        private static void AssertDirectoryContains(string dirPath, params string[] fileNames)
        {
            Directory.Exists(dirPath);
            var actualFileNames = Directory.GetFiles(dirPath).Select(Path.GetFileName);
            actualFileNames.Should().BeEquivalentTo(fileNames);
        }

        private static void AssertExpectedLocalSetting(AnalysisConfig actualConfig, string key, string expectedValue)
        {
            var found = Property.TryGetProperty(key, actualConfig.LocalSettings, out var actualProperty);

            found.Should().BeTrue("Failed to find the expected local setting: {0}", key);
            actualProperty.Value.Should().Be(expectedValue, "Unexpected property value. Key: {0}", key);
        }

        private static void AssertExpectedServerSetting(AnalysisConfig actualConfig, string key, string expectedValue)
        {
            var found = Property.TryGetProperty(key, actualConfig.ServerSettings, out var actualProperty);

            found.Should().BeTrue("Failed to find the expected server setting: {0}", key);
            actualProperty.Value.Should().Be(expectedValue, "Unexpected property value. Key: {0}", key);
        }

        private static void AssertDirectoryExists(string path) =>
            Directory.Exists(path).Should().BeTrue("Expected directory does not exist: {0}", path);

        private sealed class TestScope : IDisposable
        {
            public readonly string WorkingDir;

            private readonly WorkingDirectoryScope workingDirectory;
            private readonly EnvironmentVariableScope environmentScope;

            public TestScope(TestContext context)
            {
                WorkingDir = TestUtils.CreateTestSpecificFolderWithSubPaths(context);
                workingDirectory = new WorkingDirectoryScope(WorkingDir);
                environmentScope = PreprocessTestUtils.CreateValidNonTeamBuildScope();
            }

            public void Dispose()
            {
                workingDirectory.Dispose();
                environmentScope.Dispose();
            }
        }
    }
}
