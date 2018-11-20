using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace CLMS.Framework.Powershell
{
    public static class Commander
    {
        /// <summary>
        /// Port number for Remote PowerShell invokation without SSL
        /// </summary>
        public static int Port = 5985;

        /// <summary>
        /// Port number for Remote PowerShell invokation without SSL
        /// </summary>
        public static int SSLPort = 5986;

        public static String ShellURI = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";

        public static String AppName = "/wsman";

        /// <summary>
        /// Opens a file, reads it to the end and returns a string
        /// representation of its contents.
        /// </summary>
        /// <param name="fileName">Full path of the file to read</param>
        /// <returns>String object containing all contents of the file</returns>
        private static string ReadFile(string fileName, bool mandatoryContent = true)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"File not found: {fileName}");

            string strContent;

            using (var streamReader = new StreamReader(fileName))
            {
                strContent = streamReader.ReadToEnd();
            }

            if(mandatoryContent && string.IsNullOrWhiteSpace(strContent))
            {
                throw new EndOfStreamException($"File [{fileName}] seems to be empty");
            }

            return strContent;
        }

        private static Collection<PSObject> RunPowerShellScript(string scriptName, Runspace runspace, Dictionary<string, object> scriptArguments = null, Boolean keepAlive = false)
        {
            var commandString = ReadFile(scriptName);

            Collection<PSObject> results;

            try
            {
                try
                {
                    if (!runspace.RunspaceStateInfo.State.Equals(RunspaceState.Opened))
                        runspace.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception("Exception caught while Opening Powershell Runspace: " + ex.ToString(), ex);
                }

                using (Pipeline pipeline = runspace.CreatePipeline())
                {
                    var command = new Command(commandString, true);
                    if (scriptArguments != null && scriptArguments.Any())
                        foreach (var arg in scriptArguments)
                        {
                            command.Parameters.Add(new CommandParameter(arg.Key, arg.Value));
                        }


                    pipeline.Commands.Add(command);

                    try
                    {
                        results = pipeline.Invoke();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            String.Format("An exception was caught while Invoking Script '{0}': " + ex.ToString(),
                                scriptName), ex);
                    }
                }

            }
            finally
            {
                //todo
                //if((!keepAlive) && (!runspace.RunspaceStateInfo.State.Equals(RunspaceState.Closed)))
                if ((true) && (!runspace.RunspaceStateInfo.State.Equals(RunspaceState.Closed)))
                    runspace.Close();
            }

            return results;
        }//end RunPowerShellScript()

        public static Collection<PSObject> RunPowerShellScript(string scriptName, WSManConnectionInfo wsManconnectionInfo, Dictionary<string, object> scriptArguments = null)
        {
            Runspace runspace = RunspaceFactory.CreateRunspace(wsManconnectionInfo);
            return RunPowerShellScript(scriptName, runspace, scriptArguments);
        }//end RunPowerShellScript()

        public static Collection<PSObject> RunPowerShellScript(string scriptName, Dictionary<string, object> scriptArguments = null)
        {
            Runspace runspace = RunspaceFactory.CreateRunspace();

            return RunPowerShellScript(scriptName, runspace, scriptArguments);
        }//end RunPowerShellScript()

        public static Collection<PSObject> RunPowerShellScript(string scriptName, Runspace runspace, Dictionary<string, object> scriptArguments = null)
        {
            return RunPowerShellScript(scriptName, runspace, scriptArguments, true);
        }//end RunPowerShellScript()


    }//end public static class PowerShellExtensions
}
