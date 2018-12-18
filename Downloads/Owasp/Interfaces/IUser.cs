// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Interfaces.IUser
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using System;
using System.Collections;

namespace Owasp.Esapi.Interfaces
{
  public interface IUser
  {
    string AccountName { get; set; }

    string CsrfToken { get; }

    int FailedLoginCount { get; }

    string RememberToken { get; }

    ArrayList Roles { get; set; }

    string ScreenName { get; set; }

    bool Anonymous { get; }

    bool Enabled { get; }

    bool Expired { get; }

    bool Locked { get; }

    bool LoggedIn { get; }

    DateTime ExpirationTime { get; set; }

    void AddRole(string role);

    void AddRoles(ArrayList newRoles);

    void ChangePassword(string oldPassword, string newPassword1, string newPassword2);

    void Disable();

    void Enable();

    string GetLastHostAddress();

    DateTime GetLastFailedLoginTime();

    DateTime GetLastLoginTime();

    DateTime GetLastPasswordChangeTime();

    void IncrementFailedLoginCount();

    bool IsInRole(string role);

    bool IsFirstRequest();

    bool IsSessionAbsoluteTimeout();

    bool IsSessionTimeout();

    void Lock();

    void Logout();

    void RemoveRole(string role);

    string ResetCsrfToken();

    string ResetRememberToken();

    void Unlock();

    bool VerifyPassword(string password);

    void LoginWithPassword(string password);
  }
}
