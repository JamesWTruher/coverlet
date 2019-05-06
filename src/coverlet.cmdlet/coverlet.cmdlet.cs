using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Coverlet.Console.Logging;
using Coverlet.Core;
using Coverlet.Core.Enums;
using Coverlet.Core.Reporters;

using System.Management.Automation;

namespace Coverlet.Cmdlet
{
    [Cmdlet(VerbsLifecycle.Invoke, "Coverlet")]
    public class InvokeCodeCoverageCommand : PSCmdlet
    {
        [Parameter(Mandatory=true)]
        [ValidateNotNullOrEmpty]
        public string TestAssembly { get; set; }
        
        [Parameter(Mandatory=true)]
        [ValidateNotNullOrEmpty]
        public string TestRunner { get; set; }

        [Parameter()]
        public string TestArguments { get; set; }

        [Parameter()]
        public string OutputFileName { get; set; } = "/tmp/Coverage.json";

        [Parameter()]
        [ValidateSet("Json", "Lcov", "OpenCover", "Cobertura", "TeamCity")]
        public string OutputFormat { get; set; } = "json";

        [Parameter()]
        public string[] ExcludeFilter { get; set; }
        [Parameter()]
        public string[] IncludeFilter { get; set; }
        [Parameter()]
        public string[] ExcludeSource { get; set; }
        [Parameter()]
        public string[] IncludeSource { get; set; }
        [Parameter()]
        public string[] IncludeDirectory { get; set; }
        [Parameter()]
        public string[] ExcludeAttribute { get; set; }
        [Parameter()]
        public SwitchParameter IncludeTestAssembly { get; set; }
        [Parameter()]
        public SwitchParameter SingleHitOnly { get; set; }
        [Parameter()]
        public string MergeWith { get; set; }

        [Parameter()]
        public SwitchParameter IncludeSummary;
        private Coverage coverage;
        private CmdletLogger logger;
        private string currentLocation;

        protected override void BeginProcessing()
        {
            currentLocation = Environment.CurrentDirectory;
            // If the location
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
            logger = new CmdletLogger(this);
            logger.WriteVerbose("begin!");
            coverage = new Coverage(
                    TestAssembly,
                    IncludeFilter, IncludeDirectory,
                    ExcludeFilter, ExcludeSource, ExcludeAttribute,
                    IncludeTestAssembly,
                    SingleHitOnly, MergeWith, IncludeTestAssembly,
                    logger);
            coverage.PrepareModules();

            Process process = new Process();
            process.StartInfo.FileName = TestRunner;
            process.StartInfo.Arguments = TestArguments;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += (sender, eventArgs) =>
            {
                if (!string.IsNullOrEmpty(eventArgs.Data))
                    logger.LogInformation(eventArgs.Data, this);
            };

            process.ErrorDataReceived += (sender, eventArgs) =>
            {
                if (!string.IsNullOrEmpty(eventArgs.Data))
                    logger.LogError(eventArgs.Data);
            };

            process.Start();

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.WaitForExit();
        }

        protected override void EndProcessing()
        {
            var result = coverage.GetCoverageResult();
            var vv = AssemblyCoverageHelper.ConvertCoverageData(result, IncludeSummary: IncludeSummary);
            // WriteObject(result);
            WriteObject(vv);
            var reporter = new ReporterFactory(OutputFormat).CreateReporter();
            File.WriteAllText(OutputFileName, reporter.Report(result));
            logger.WriteVerbose("end!");
        }
    
    }

}