// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.HttpUtilities
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using Owasp.Esapi.Errors;
using Owasp.Esapi.Interfaces;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;

namespace Owasp.Esapi
{
  public class HttpUtilities 
  {
    private static readonly Logger logger = Logger.GetLogger("Esapi", nameof (HttpUtilities));
    internal int maxBytes;

    private void InitBlock()
    {
      this.maxBytes = Owasp.Esapi.Esapi.SecurityConfiguration().AllowedFileUploadSize;
    }

    public bool SecureChannel
    {
      get
      {
        return ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentRequest.RawUrl.ToString()[4] == 's';
      }
    }

    public HttpUtilities()
    {
      this.InitBlock();
    }

    public string AddCsrfToken(string href)
    {
      User currentUser = (User) Owasp.Esapi.Esapi.Authenticator().GetCurrentUser();
      if (currentUser.Anonymous || currentUser == null)
        return href;
      if (href.IndexOf('?') != -1 || href.IndexOf('&') != -1)
        return href + "&" + currentUser.CsrfToken;
      return href + "?" + currentUser.CsrfToken;
    }

    public void SafeAddCookie(string name, string cookieValue, int maxAge, string domain, string path)
    {
      HttpResponse currentResponse = ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentResponse;
      if (currentResponse == null)
        throw new NullReferenceException("Can't set response header until current response is set, typically via login");
      string str1 = name + "=" + cookieValue;
      if (maxAge != -1)
        str1 = str1 + "; Max-Age=" + (object) maxAge;
      if (domain != null)
        str1 = str1 + "; Domain=" + domain;
      if (path != null)
        str1 = str1 + "; Path=" + path;
      string str2 = str1 + "; Secure; HttpOnly";
      currentResponse.AppendHeader("Set-Cookie", str2);
    }

    public void SafeAddHeader(string name, string val)
    {
      HttpResponse currentResponse = ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentResponse;
      if (currentResponse == null)
        throw new NullReferenceException("Can't set response header until current response is set, typically via login");
      if (!((SecurityConfiguration) Owasp.Esapi.Esapi.SecurityConfiguration()).GetValidationPattern("HTTPHeaderName").IsMatch(name))
        throw new ValidationException("Invalid header", "Attempt to set a header name that violates the global rule in Esapi.properties: " + name);
      Regex validationPattern = ((SecurityConfiguration) Owasp.Esapi.Esapi.SecurityConfiguration()).GetValidationPattern("HTTPHeaderValue");
      if (!validationPattern.IsMatch(val))
        throw new ValidationException("Invalid header", "Attempt to set a header value that violates the global rule in Esapi.properties: " + (object) validationPattern);
      currentResponse.AppendHeader(name, val);
    }

    public void SafeSetHeader(string name, string value)
    {
      HttpResponse currentResponse = ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentResponse;
      if (currentResponse == null)
        throw new NullReferenceException("Can't set response header until current response is set, typically via login");
      if (!((SecurityConfiguration) Owasp.Esapi.Esapi.SecurityConfiguration()).GetValidationPattern("HTTPHeaderName").IsMatch(name))
        throw new ValidationException("Invalid header", "Attempt to set a header name that violates the global rule in Esapi.properties: " + name);
      Regex validationPattern = ((SecurityConfiguration) Owasp.Esapi.Esapi.SecurityConfiguration()).GetValidationPattern("HTTPHeaderValue");
      if (!validationPattern.IsMatch(value))
        throw new ValidationException("Invalid header", "Attempt to set a header value that violates the global rule in Esapi.properties: " + (object) validationPattern);
      currentResponse.Headers[name] = value;
    }

    public string SafeEncodeURL(string url)
    {
      return url;
    }

    public string SafeEncodeRedirectURL(string url)
    {
      return url;
    }

    public HttpSessionState ChangeSessionIdentifier()
    {
      HttpRequest currentRequest = ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentRequest;
      HttpResponse currentResponse = ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentResponse;
      HttpSessionState currentSession = ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentSession;
      IDictionary dictionary = (IDictionary) new Hashtable();
      IEnumerator enumerator = ((IEnumerable) currentSession).GetEnumerator();
      while (enumerator != null && enumerator.MoveNext())
      {
        string current = (string) enumerator.Current;
        object obj = currentSession;
        dictionary[(object) current] = obj;
      }
      currentSession.Abandon();
      currentResponse.SetCookie(new HttpCookie("ASP.NET_SessionId", ""));
      foreach (DictionaryEntry dictionaryEntry in new ArrayList((ICollection) dictionary))
        currentSession.Add((string) dictionaryEntry.Key, dictionaryEntry.Value);
      return currentSession;
    }

        public void VerifyCsrfToken()
        {
            HttpRequest currentRequest = ((Authenticator)Owasp.Esapi.Esapi.Authenticator()).CurrentRequest;
            User currentUser = (User)Owasp.Esapi.Esapi.Authenticator().GetCurrentUser();
            if (!currentUser.IsFirstRequest() && currentRequest.Params[currentUser.CsrfToken] == null)
        throw new IntrusionException("Authentication failed", "Possibly forged HTTP request without proper CSRF token detected");
    }

    public string DecryptHiddenField(string encrypted)
    {
      try
      {
        return Owasp.Esapi.Esapi.Encryptor().Decrypt(encrypted);
      }
      catch (EncryptionException ex)
      {
        throw new IntrusionException("Invalid request", "Tampering detected. Hidden field data did not decrypt properly.", (Exception) ex);
      }
    }

    public IDictionary DecryptQueryString(string encrypted)
    {
      return this.QueryToMap(Owasp.Esapi.Esapi.Encryptor().Decrypt(encrypted));
    }

    public IDictionary DecryptStateFromCookie()
    {
      return this.QueryToMap(Owasp.Esapi.Esapi.Encryptor().Decrypt(((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentRequest.Cookies["state"].Value));
    }

    public string EncryptHiddenField(string fieldValue)
    {
      return Owasp.Esapi.Esapi.Encryptor().Encrypt(fieldValue);
    }

    public string EncryptQueryString(string query)
    {
      return Owasp.Esapi.Esapi.Encryptor().Encrypt(query);
    }

    public void EncryptStateInCookie(IDictionary cleartext)
    {
      StringBuilder stringBuilder = new StringBuilder();
      IEnumerator enumerator = new ArrayList((ICollection) cleartext).GetEnumerator();
      bool flag = true;
      while (enumerator.MoveNext())
      {
        try
        {
          if (!flag)
            stringBuilder.Append("&");
          else
            flag = false;
          DictionaryEntry current = (DictionaryEntry) enumerator.Current;
          string str1 = Owasp.Esapi.Esapi.Encoder().EncodeForUrl(current.Key.ToString());
          string str2 = Owasp.Esapi.Esapi.Encoder().EncodeForUrl(current.Value.ToString());
          stringBuilder.Append(str1 + "=" + str2);
        }
        catch (EncodingException ex)
        {
        }
      }
      this.SafeAddCookie("state", Owasp.Esapi.Esapi.Encryptor().Encrypt(stringBuilder.ToString()), -1, (string) null, (string) null);
    }

    public IList GetSafeFileUploads(DirectoryInfo tempDir, DirectoryInfo finalDir)
    {
      ArrayList arrayList = new ArrayList();
      try
      {
        if (!tempDir.Exists)
          tempDir.Create();
        if (!finalDir.Exists)
          finalDir.Create();
        HttpFileCollection files = ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentRequest.Files;
        if (files.AllKeys.Length == 0)
          throw new ValidationUploadException("Upload failed", "Not a multipart request");
        foreach (string allKey in files.AllKeys)
        {
          HttpPostedFile ihttpPostedFile = files.Get(allKey);
          if (ihttpPostedFile.FileName != null && !ihttpPostedFile.FileName.Equals(""))
          {
            string[] strArray1 = Regex.Split(ihttpPostedFile.FileName, "[\\/\\\\]");
            string input = strArray1[strArray1.Length - 1];
            if (!Owasp.Esapi.Esapi.Validator().IsValidFileName("upload", input, false))
            {
              string str = "";
              foreach (string allowedFileExtension in (IEnumerable) Owasp.Esapi.Esapi.SecurityConfiguration().AllowedFileExtensions)
                str = str + allowedFileExtension + "|";
              throw new ValidationUploadException("Upload only simple filenames with the following extensions " + str, "Invalid filename for upload");
            }
            HttpUtilities.logger.LogCritical(ILogger_Fields.SECURITY, "File upload requested: " + input);
            FileInfo fileInfo = new FileInfo(finalDir.ToString() + "\\" + input);
            if (fileInfo.Exists)
            {
              string[] strArray2 = Regex.Split(input, "\\./");
              string str1 = "";
              if (strArray2.Length > 1)
                str1 = strArray2[strArray2.Length - 1];
              string str2 = input.Substring(0, input.Length - str1.Length);
              fileInfo = new FileInfo(finalDir.ToString() + "\\" + str2 + (object) Guid.NewGuid() + "." + str1);
            }
            ihttpPostedFile.SaveAs(fileInfo.FullName);
            arrayList.Add((object) fileInfo);
            HttpUtilities.logger.LogCritical(ILogger_Fields.SECURITY, "File successfully uploaded: " + (object) fileInfo);
          }
        }
        HttpUtilities.logger.LogCritical(ILogger_Fields.SECURITY, "File successfully uploaded: ");
      }
      catch (Exception ex)
      {
        if (ex is ValidationUploadException)
          throw (ValidationException) ex;
        throw new ValidationUploadException("Upload failure", "Problem during upload");
      }
      return (IList) arrayList;
    }

    public void KillAllCookies()
    {
      HttpCookieCollection cookies = ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentRequest.Cookies;
      if (cookies == null)
        return;
      foreach (string name in (NameObjectCollectionBase) cookies)
        this.KillCookie(name);
    }

    public void KillCookie(string name)
    {
      HttpRequest currentRequest = ((Authenticator)Esapi.Authenticator()).CurrentRequest;
      HttpResponse currentResponse = ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentResponse;
            HttpCookieCollection cookies = currentRequest.Cookies;
      if (cookies == null)
        return;
      foreach (string str1 in (NameObjectCollectionBase) cookies)
      {
        if (str1.Equals(name))
        {
          string applicationPath = currentRequest.PhysicalPath;
          string str2 = name + "=deleted; Max-Age=0; Path=" + applicationPath;
          currentResponse.AppendHeader("Set-Cookie", str2);
        }
      }
    }

    private IDictionary QueryToMap(string query)
    {
      SortedList sortedList = new SortedList();
      foreach (string input in Regex.Split(query, "&"))
      {
        try
        {
          string[] strArray = Regex.Split(input, "=");
          string str1 = Owasp.Esapi.Esapi.Encoder().DecodeFromUrl(strArray[0]);
          string str2 = Owasp.Esapi.Esapi.Encoder().DecodeFromUrl(strArray[1]);
          sortedList[(object) str1] = (object) str2;
        }
        catch (EncodingException ex)
        {
        }
      }
      return (IDictionary) sortedList;
    }

    public void SafeSendForward(string context, string location)
    {
      HttpRequest currentRequest = ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentRequest;
      HttpResponse currentResponse = ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentResponse;
      throw new NotImplementedException();
    }

    public void SafeSendRedirect(string context, string location)
    {
      HttpResponse currentResponse = ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentResponse;
      if (currentResponse == null)
        throw new NullReferenceException("Can't set response header until current response is set, typically via login");
      if (!Owasp.Esapi.Esapi.Validator().IsValidRedirectLocation(context, location, false))
        throw new ValidationException("Redirect failed", "Bad redirect location: " + location);
      currentResponse.Redirect(location);
    }

    public void SafeSetContentType()
    {
      HttpResponse currentResponse = ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentResponse;
      if (currentResponse == null)
        throw new NullReferenceException("Can't set response header until current response is set, typically via login");
      currentResponse.ContentType=(((SecurityConfiguration) Owasp.Esapi.Esapi.SecurityConfiguration()).ResponseContentType);
    }

    public void SetNoCacheHeaders()
    {
      HttpResponse currentResponse = ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentResponse;
      if (currentResponse == null)
        throw new NullReferenceException("Can't set response header until current response is set, typically via login");
      currentResponse.AppendHeader("Cache-Control", "no-store");
      currentResponse.AppendHeader("Cache-Control", "no-cache");
      currentResponse.AppendHeader("Cache-Control", "must-revalidate");
      currentResponse.AppendHeader("Pragma", "no-cache");
      currentResponse.AppendHeader("Expires", DateTime.MinValue.ToString("r"));
    }

    public void checkCSRFToken()
    {
      throw new Exception("The method or operation is not implemented.");
    }
  }
}
