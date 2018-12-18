// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Executor
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using Owasp.Esapi.Errors;
using Owasp.Esapi.Interfaces;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace Owasp.Esapi
{
  public class Executor : IExecutor
  {
    private static readonly Logger logger = Logger.GetLogger("ESAPI", nameof (Executor));
    private readonly int MAX_SYSTEM_COMMAND_LENGTH = 2500;

    public string ExecuteSystemCommand(FileInfo executable, IList parameters, FileInfo workdir, int timeoutSeconds)
    {
      StreamReader streamReader = (StreamReader) null;
      try
      {
        Executor.logger.LogTrace(ILogger_Fields.SECURITY, "Initiating executable: " + (object) executable + " " + parameters.ToString() + " in " + (object) workdir);
        IValidator validator = Owasp.Esapi.Esapi.Validator();
        if (!executable.FullName.Equals(executable.FullName))
          throw new ExecutorException("Execution failure", "Invalid path to executable file: " + (object) executable);
        if (!File.Exists(executable.FullName) && !Directory.Exists(executable.FullName))
          throw new ExecutorException("Execution failure", "No such executable: " + (object) executable);
        foreach (string parameter in (IEnumerable) parameters)
        {
          if (!validator.IsValidInput("fixme", "SystemCommand", parameter, this.MAX_SYSTEM_COMMAND_LENGTH, false))
            throw new ExecutorException("Execution failure", "Illegal characters in parameter to executable: " + parameter);
        }
        if (!File.Exists(workdir.FullName) && !Directory.Exists(workdir.FullName))
          throw new ExecutorException("Execution failure", "No such working directory for running executable: " + workdir.FullName);
        Process process = Process.Start(new ProcessStartInfo()
        {
          CreateNoWindow = true,
          FileName = executable.FullName,
          Arguments = parameters.ToString(),
          RedirectStandardOutput = true,
          UseShellExecute = false
        });
        Executor.logger.LogTrace(ILogger_Fields.SECURITY, "System command successful: " + parameters.ToString());
        return process.StandardOutput.ReadToEnd();
      }
      catch (Exception ex)
      {
        throw new ExecutorException("Execution failure", "Exception thrown during execution of system command: " + ex.Message, ex);
      }
      finally
      {
        try
        {
          streamReader?.Close();
        }
        catch (IOException ex)
        {
        }
      }
    }
  }
}
