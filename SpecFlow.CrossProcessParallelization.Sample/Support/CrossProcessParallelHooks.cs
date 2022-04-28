using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Text;
using NUnit.Framework;

namespace SpecFlow.CrossProcessParallelization.Sample.Support
{
    [Binding]
    internal class CrossProcessParallelHooks
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly FeatureContext _featureContext;
        private static readonly Semaphore _lockSemaphore = new(1, 1, "SpecFlowParallel" + GetRunIdentifier());
        private static readonly ConcurrentBag<string> _processedScenarios = new();
        public CrossProcessParallelHooks(ScenarioContext scenarioContext, FeatureContext featureContext)
        {
            _scenarioContext = scenarioContext;
            _featureContext = featureContext;
        }

        [BeforeScenario(Order = -1)]
        public void CanRunTest()
        {
            var runIdentifier = GetRunIdentifier();
            if (string.IsNullOrEmpty(runIdentifier))
                return; // run by single process
            var scenarioReferenceName = GetScenarioSpecificFileName(".lock");
            var lockFolderName = Path.Combine(Path.GetTempPath(), "SpecFlowParallel", runIdentifier);
            var lockFile = Path.Combine(lockFolderName, scenarioReferenceName);

            if (_processedScenarios.Contains(scenarioReferenceName))
                return; // this scenario has been processed by this test process -- was this a retry?

            CheckLock(lockFile); // if the test is already taken -> skip

            if (!_lockSemaphore.WaitOne(TimeSpan.FromSeconds(10)))
                Assert.Fail("Unable to receive cross process lock");
            try
            {
                CheckLock(lockFile);
                TouchFile(lockFolderName, lockFile);
                _processedScenarios.Add(scenarioReferenceName);
            }
            finally
            {
                _lockSemaphore.Release();
            }
        }

        private static string GetRunIdentifier()
        {
            return Environment.GetEnvironmentVariable("SPECFLOW_TEST_RUN_ID") ?? "";
        }

        public string GetScenarioSpecificFileName(string extension = "")
        {
            var baseFileName = $"{ToPath(_featureContext.FeatureInfo.Title)}_{ToPath(_scenarioContext.ScenarioInfo.Title)}";
            if (_scenarioContext.ScenarioInfo.Arguments != null && _scenarioContext.ScenarioInfo.Arguments.Count > 0)
            {
                foreach (DictionaryEntry entry in _scenarioContext.ScenarioInfo.Arguments)
                {
                    baseFileName += $"_{entry.Key}-{entry.Value}";
                }
            }
            return baseFileName + extension;
        }

        /// <summary>
        /// Makes string path-compatible, ie removes characters not allowed in path and replaces whitespace with '_'
        /// </summary>
        public string ToPath(string s)
        {
            var builder = new StringBuilder(s);
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                builder.Replace(invalidChar.ToString(), "");
            }
            builder.Replace(' ', '_');
            return builder.ToString();
        }

        private static void TouchFile(string lockFolderName, string lockFile)
        {
            if (!Directory.Exists(lockFolderName))
                Directory.CreateDirectory(lockFolderName);
            File.WriteAllText(lockFile, "");
        }

        private static void CheckLock(string lockFile)
        {
            if (File.Exists(lockFile))
                Assert.Ignore("This test was processed by another test process");
        }
    }
}
