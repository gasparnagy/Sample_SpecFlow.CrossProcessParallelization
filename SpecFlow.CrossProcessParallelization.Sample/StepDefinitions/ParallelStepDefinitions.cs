using System;
using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;

namespace SpecFlow.CrossProcessParallelization.Sample.StepDefinitions
{
    [Binding]
    public class ParallelStepDefinitions
    {
        public static readonly string UniqueId = DateTime.Now.ToString("s", CultureInfo.InvariantCulture).Replace(":", "");

        [When(@"the test (.*) is executed")]
        public void WhenTheTestIsExecuted(int testId)
        {
            Thread.Sleep(2000); // simulate slow test
            var processId = Process.GetCurrentProcess().Id;
            var outputFilePath = Path.Combine(OutputFolder, $"result-{testId}-by-{processId}.txt");
            Assert.IsFalse(File.Exists(outputFilePath), $"{testId} has been processed already");
            File.WriteAllText(outputFilePath, testId.ToString());
        }

        public string OutputFolder
        {
            get
            {
                var outputFolder = Path.Combine(Directory.GetCurrentDirectory(), UniqueId);
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);
                return outputFolder;
            }
        }
    }
}
