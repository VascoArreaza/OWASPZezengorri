// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Interfaces.ILogger
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using System;
using System.Collections;

namespace Owasp.Esapi.Interfaces
{
  public interface ILogger
  {
    void LogHttpRequest();

    void LogHttpRequest(IList parameterNamesToObfuscate);

    void LogCritical(string type, string message);

    void LogCritical(string type, string message, Exception throwable);

    void LogDebug(string type, string message);

    void LogDebug(string type, string message, Exception throwable);

    void LogError(string type, string message);

    void LogError(string type, string message, Exception throwable);

    void LogSuccess(string type, string message);

    void LogSuccess(string type, string message, Exception throwable);

    void LogTrace(string type, string message);

    void LogTrace(string type, string message, Exception throwable);

    void LogWarning(string type, string message);

    void LogWarning(string type, string message, Exception throwable);
  }
}
