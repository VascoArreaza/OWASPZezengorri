// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Errors.EncodingException
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using System;

namespace Owasp.Esapi.Errors
{
  [Serializable]
  public class EncodingException : EnterpriseSecurityException
  {
    private const long _serialVersionUID = 1;

    protected internal EncodingException()
    {
    }

    public EncodingException(string userMessage, string logMessage)
      : base(userMessage, logMessage)
    {
    }

    public EncodingException(string userMessage, string logMessage, Exception cause)
      : base(userMessage, logMessage, cause)
    {
    }
  }
}
