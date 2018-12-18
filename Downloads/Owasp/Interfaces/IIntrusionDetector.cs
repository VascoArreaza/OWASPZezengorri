// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Interfaces.IIntrusionDetector
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using System;

namespace Owasp.Esapi.Interfaces
{
  public interface IIntrusionDetector
  {
    void AddException(Exception exception);

    void AddEvent(string eventName);
  }
}
