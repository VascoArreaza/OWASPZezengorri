// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Interfaces.IHttpUtilities
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll


using System.Collections;
using System.IO;
using System.Web.SessionState;

namespace Owasp.Esapi.Interfaces
{
  public interface IHttpUtilities
  {
    bool SecureChannel { get; }

    string AddCsrfToken(string href);

    void SafeAddCookie(string name, string value, int maxAge, string domain, string path);

    void SafeAddHeader(string name, string value);

    HttpSessionState ChangeSessionIdentifier();

    void VerifyCsrfToken();

    IList GetSafeFileUploads(DirectoryInfo tempDir, DirectoryInfo finalDir);

    void KillAllCookies();

    void KillCookie(string name);

    void SafeSendRedirect(string context, string location);

    void SafeSetContentType();

    void SetNoCacheHeaders();

    void EncryptStateInCookie(IDictionary cleartext);

    IDictionary DecryptStateFromCookie();

    string EncryptHiddenField(string value);

    string DecryptHiddenField(string encrypted);

    string EncryptQueryString(string query);

    IDictionary DecryptQueryString(string encrypted);

    void checkCSRFToken();
  }
}
