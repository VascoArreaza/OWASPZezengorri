// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Interfaces.IEncryptor
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

namespace Owasp.Esapi.Interfaces
{
  public interface IEncryptor
  {
    long TimeStamp { get; }

    string Hash(string plaintext, string salt);

    string Encrypt(string plaintext);

    string Decrypt(string ciphertext);

    string Sign(string data);

    bool VerifySignature(string signature, string data);

    string Seal(string data, long timestamp);

    bool VerifySeal(string seal, string data);
  }
}
