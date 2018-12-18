// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Interfaces.ISecurityConfiguration
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using System.Collections;
using System.IO;

namespace Owasp.Esapi.Interfaces
{
  public interface ISecurityConfiguration
  {
    string MasterPassword { get; }

    FileInfo Keystore { get; }

    byte[] MasterSalt { get; }

    IList AllowedFileExtensions { get; }

    int AllowedFileUploadSize { get; }

    string PasswordParameterName { get; }

    string UsernameParameterName { get; }

    string EncryptionAlgorithm { get; }

    string HashAlgorithm { get; }

    string CharacterEncoding { get; }

    string DigitalSignatureAlgorithm { get; }

    string RandomAlgorithm { get; }

    int AllowedLoginAttempts { get; }

    int MaxOldPasswordHashes { get; }

    FileInfo ResourceDirectory { get; set; }

    bool RequireSecureChannel { get; }

    Threshold GetQuota(string eventName);
  }
}
