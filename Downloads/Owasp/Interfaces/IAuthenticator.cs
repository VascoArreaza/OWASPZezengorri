// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Interfaces.IAuthenticator
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using System.Collections;

namespace Owasp.Esapi.Interfaces
{
    public interface IAuthenticator
  {
    IList GetUserNames();

    IUser Login();

    IUser CreateUser(string accountName, string password1, string password2);

    string GenerateStrongPassword();

    string GenerateStrongPassword(string oldPassword, IUser user);

    IUser GetUser(string accountName);

    IUser GetCurrentUser();

    void SetCurrentUser(IUser user);

    string HashPassword(string password, string accountName);

    void RemoveUser(string accountName);

    void VerifyAccountNameStrength(string accountName);

    void VerifyPasswordStrength(string oldPassword, string newPassword);

    bool Exists(string accountName);

    IUser GetUserFromSession(IHttpRequest request);
  }
}
