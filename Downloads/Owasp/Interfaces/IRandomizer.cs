// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Interfaces.IRandomizer
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

namespace Owasp.Esapi.Interfaces
{
  public interface IRandomizer
  {
    bool RandomBoolean { get; }

    string RandomGUID { get; }

    string GetRandomString(int length, char[] characterSet);

    int GetRandomInteger(int min, int max);

    string GetRandomFilename(string extension);

    float GetRandomReal(float min, float max);
  }
}
