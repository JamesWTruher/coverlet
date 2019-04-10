using System;
using Coverlet.Core.Logging;
using System.Management.Automation;

namespace Coverlet.Console.Logging
{
    public class CmdletLogger : ILogger
    {
        public CmdletLogger(PSCmdlet writer) {
            cmdlet = writer;
        }
        private PSCmdlet cmdlet;
        private static readonly object _sync = new object();

        public void LogError(string message) {
            lock(_sync) {
                cmdlet.WriteError(new ErrorRecord(
                        new InvalidOperationException(message),
                        "InvalidOperation",
                        ErrorCategory.InvalidOperation,
                        null
                    ));
            }
        }

        public void LogError(Exception exception) {
            lock(_sync) {
                cmdlet.WriteError(new ErrorRecord(
                        exception,
                        "InvalidOperation",
                        ErrorCategory.InvalidOperation,
                        null
                    ));
            }
        }

        public void LogInformation(string message) 
        {
            lock(_sync) {
                cmdlet.WriteInformation(new InformationRecord(message, String.Empty));
            }
        }
        public void LogInformation(string message, PSCmdlet cmdlet)
        {
            lock(_sync) {
                cmdlet.WriteInformation(new InformationRecord(message, cmdlet.MyInvocation.InvocationName));
            }
        }

        public void WriteVerbose(string message)
        {
            lock(_sync) {
                cmdlet.WriteVerbose(message);
            }
        }
        public void LogVerbose(string message)
        {
            lock(_sync) {
                cmdlet.WriteVerbose(message);
            }
        }

        public void LogWarning(string message)
        {
            lock(_sync) {
                cmdlet.WriteWarning(message);
            }
        }

    }
}