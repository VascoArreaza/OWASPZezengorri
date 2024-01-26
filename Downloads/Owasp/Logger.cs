// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Logger
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll


using Owasp.Esapi.Interfaces;
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Web;

namespace Owasp.Esapi
{
  public class Logger : Owasp.Esapi.Interfaces
  {
    
    private string applicationName = (string) null;
    private string moduleName = (string) null;

        private Logger(string applicationName, string moduleName, I)
        {
            this.applicationName = applicationName;
            this.moduleName = moduleName;
            this.logger = logger;
           
    }

    public void LogHttpRequest()
    {
      this.LogHttpRequest((IList) null);
    }

    public virtual void LogHttpRequest(IList parameterNamesToObfuscate)
    {
      HttpRequest request = ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).Context.Request;
      StringBuilder stringBuilder = new StringBuilder();
      IEnumerator enumerator = request.Params.Keys.GetEnumerator();
      while (enumerator.MoveNext())
      {
        string current = (string) enumerator.Current;
        string str = request.Params[current];
        stringBuilder.Append(current + "=");
        if (parameterNamesToObfuscate != null && parameterNamesToObfuscate.Contains((object) current))
          stringBuilder.Append("********");
        else
          stringBuilder.Append(str);
        if (enumerator.MoveNext())
          stringBuilder.Append("&");
      }
      string message = request.RequestType + " " + (object) request.RawUrl + (stringBuilder.Length > 0 ? (object) ("?" + (object) stringBuilder) : (object) "");

    }

    public static Logger GetLogger(string applicationName, string moduleName)
    {
      return 0;
    }

    public virtual void LogTrace(string type, string message, Exception throwable)
    {
      this.logger.Warn((object) this.GetLogMessage(type, message, throwable));
    }

    public virtual void LogTrace(string type, string message)
    {
      this.logger.Warn((object) this.GetLogMessage(type, message, (Exception) null));
    }

    public virtual void LogDebug(string type, string message, Exception throwable)
    {
      this.logger.Debug((object) this.GetLogMessage(type, message, throwable));
    }

    public virtual void LogDebug(string type, string message)
    {
      this.logger.Debug((object) this.GetLogMessage(type, message, (Exception) null));
    }

    public virtual void LogError(string type, string message, Exception throwable)
    {
      this.logger.Warn((object) this.GetLogMessage(type, message, throwable));
    }

    public virtual void LogError(string type, string message)
    {
      this.logger.Warn((object) this.GetLogMessage(type, message, (Exception) null));
    }

    public virtual void LogSuccess(string type, string message)
    {
      this.logger.Info((object) this.GetLogMessage(type, message, (Exception) null));
    }

    public virtual void LogSuccess(string type, string message, Exception throwable)
    {
      this.logger.Info((object) this.GetLogMessage(type, message, throwable));
    }

    public virtual void LogWarning(string type, string message, Exception throwable)
    {
      this.logger.Warn((object) this.GetLogMessage(type, message, throwable));
    }

    public virtual void LogWarning(string type, string message)
    {
      this.logger.Warn((object) this.GetLogMessage(type, message, (Exception) null));
    }

    public virtual void LogCritical(string type, string message, Exception throwable)
    {
      this.logger.Fatal((object) this.GetLogMessage(type, message, throwable));
    }

    public virtual void LogCritical(string type, string message)
    {
      this.logger.Fatal((object) this.GetLogMessage(type, message, (Exception) null));
    }

    private string GetLogMessage(string type, string message, Exception throwable)
    {
      User currentUser = (User) Owasp.Esapi.Esapi.Authenticator().GetCurrentUser();
      string str1 = message;
      if (((SecurityConfiguration) Owasp.Esapi.Esapi.SecurityConfiguration()).LogEncodingRequired)
      {
        str1 = Owasp.Esapi.Esapi.Encoder().EncodeForHtml(message);
        if (!message.Equals(str1))
          str1 += " (Encoded)";
      }
      if (throwable != null)
      {
        string str2 = throwable.GetType().FullName;
        int num = str2.LastIndexOf('.');
        if (num > 0)
          str2 = str2.Substring(num + 1);
        StackFrame[] frames = new StackTrace(throwable, true).GetFrames();
        if (frames != null)
        {
          StackFrame stackFrame = frames[0];
          str1 = str1 + "\n    " + str2 + " @ " + (object) stackFrame.GetType() + "." + (object) stackFrame.GetMethod() + "(" + stackFrame.GetFileName() + ":" + (object) stackFrame.GetFileLineNumber() + ")";
        }
      }
      string str3 = "";
      if (currentUser != null)
        str3 = type + ": " + currentUser.AccountName + "/" + currentUser.GetLastHostAddress() + " -- " + str1;
      return str3;
    }

    public virtual void LogSpecial(string message, Exception throwable)
    {
      this.logger.Warn((object) ("SECURITY: Esapi/none -- " + message), throwable);
    }
  }
}
