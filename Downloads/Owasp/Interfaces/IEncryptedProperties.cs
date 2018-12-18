// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Interfaces.IEncryptedProperties
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

namespace Owasp.Esapi.Interfaces
{
  public interface IEncryptedProperties
  {
    string GetProperty(string key);

    string SetProperty(string key, string value);
  }
}
