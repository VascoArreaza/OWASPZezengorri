// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Errors.IntrusionException
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using Owasp.Esapi.Interfaces;
using System;

namespace Owasp.Esapi.Errors
{
  [Serializable]
  public class IntrusionException : SystemException
  {
    protected internal static readonly Logger _logger = Logger.GetLogger("ESAPI", nameof (IntrusionException));
    protected internal string _logMessage = (string) null;
    private const long _serialVersionUID = 1;

    public virtual string UserMessage
    {
      get
      {
        return this.Message;
      }
    }

    public virtual string LogMessage
    {
      get
      {
        return this._logMessage;
      }
    }

    public IntrusionException()
    {
    }

    public IntrusionException(string userMessage, string logMessage)
      : base(userMessage)
    {
      this._logMessage = logMessage;
      IntrusionException._logger.LogError(ILogger_Fields.SECURITY, "INTRUSION - " + logMessage);
    }

    public IntrusionException(string userMessage, string logMessage, Exception cause)
      : base(userMessage, cause)
    {
      this._logMessage = logMessage;
      IntrusionException._logger.LogError(ILogger_Fields.SECURITY, "INTRUSION - " + logMessage, cause);
    }
  }
}
