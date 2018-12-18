// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Errors.EnterpriseSecurityException
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using System;

namespace Owasp.Esapi.Errors
{
  [Serializable]
  public class EnterpriseSecurityException : Exception
  {
    protected internal static readonly Logger _logger = Logger.GetLogger("ESAPI", nameof (EnterpriseSecurityException));
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

    protected internal EnterpriseSecurityException()
    {
    }

    public EnterpriseSecurityException(string userMessage, string logMessage)
      : base(userMessage)
    {
      this._logMessage = logMessage;
      Owasp.Esapi.Esapi.IntrusionDetector().AddException((Exception) this);
    }

    public EnterpriseSecurityException(string userMessage, string logMessage, Exception cause)
      : base(userMessage, cause)
    {
      this._logMessage = logMessage;
      Owasp.Esapi.Esapi.IntrusionDetector().AddException((Exception) this);
    }
  }
}
