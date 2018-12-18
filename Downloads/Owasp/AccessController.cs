// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.AccessController
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using Owasp.Esapi.Errors;
using Owasp.Esapi.Interfaces;
using System;
using System.Collections;
using System.IO;

namespace Owasp.Esapi
{
  public class AccessController : IAccessController
  {
    private static readonly FileInfo resourceDirectory = ((SecurityConfiguration) Owasp.Esapi.Esapi.SecurityConfiguration()).ResourceDirectory;
    private static Logger logger = Logger.GetLogger("ESAPI", nameof (AccessController));
    private IDictionary urlMap = (IDictionary) new Hashtable();
    private IDictionary functionMap = (IDictionary) new Hashtable();
    private IDictionary dataMap = (IDictionary) new Hashtable();
    private IDictionary fileMap = (IDictionary) new Hashtable();
    private IDictionary serviceMap = (IDictionary) new Hashtable();
    private AccessController.Rule deny = new AccessController.Rule();

    private void InitBlock()
    {
    }

    public bool IsAuthorizedForUrl(string url)
    {
      if (this.urlMap.Count == 0)
        this.urlMap = this.LoadRules(new FileInfo(AccessController.resourceDirectory.FullName + "\\URLAccessRules.txt"));
      return this.MatchRule(this.urlMap, url);
    }

    public bool IsAuthorizedForFunction(string functionName)
    {
      try
      {
        this.AssertAuthorizedForFunction(functionName);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public bool IsAuthorizedForData(string key)
    {
      try
      {
        this.AssertAuthorizedForData(key);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public bool IsAuthorizedForFile(string filepath)
    {
      try
      {
        this.AssertAuthorizedForFile(filepath);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public bool IsAuthorizedForService(string serviceName)
    {
      try
      {
        this.AssertAuthorizedForService(serviceName);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public void AssertAuthorizedForUrl(string url)
    {
      if (this.urlMap == null || this.urlMap.Count == 0)
        this.urlMap = this.LoadRules(new FileInfo(AccessController.resourceDirectory.ToString() + "\\URLAccessRules.txt"));
      if (!this.MatchRule(this.urlMap, url))
        throw new AccessControlException("Not authorized for URL", "Not authorized for URL: " + url);
    }

    public void AssertAuthorizedForFunction(string functionName)
    {
      if (this.functionMap == null || this.functionMap.Count == 0)
        this.functionMap = this.LoadRules(new FileInfo(AccessController.resourceDirectory.ToString() + "\\FunctionAccessRules.txt"));
      if (!this.MatchRule(this.functionMap, functionName))
        throw new AccessControlException("Not authorized for function", "Not authorized for function: " + functionName);
    }

    public void AssertAuthorizedForData(string key)
    {
      if (this.dataMap == null || this.dataMap.Count == 0)
        this.dataMap = this.LoadRules(new FileInfo(AccessController.resourceDirectory.ToString() + "\\DataAccessRules.txt"));
      if (!this.MatchRule(this.dataMap, key))
        throw new AccessControlException("Not authorized for function", "Not authorized for data: " + key);
    }

    public void AssertAuthorizedForFile(string filepath)
    {
      if (this.fileMap == null || this.fileMap.Count == 0)
        this.fileMap = this.LoadRules(new FileInfo(AccessController.resourceDirectory.ToString() + "\\FileAccessRules.txt"));
      if (!this.MatchRule(this.fileMap, filepath.Replace("\\\\", "/")))
        throw new AccessControlException("Not authorized for file", "Not authorized for file: " + filepath);
    }

    public void AssertAuthorizedForService(string serviceName)
    {
      if (this.serviceMap == null || this.serviceMap.Count == 0)
        this.serviceMap = this.LoadRules(new FileInfo(AccessController.resourceDirectory.ToString() + "\\ServiceAccessRules.txt"));
      if (!this.MatchRule(this.serviceMap, serviceName))
        throw new AccessControlException("Not authorized for service", "Not authorized for service: " + serviceName);
    }

    private bool MatchRule(IDictionary map, string path)
    {
      IList roles = (IList) Owasp.Esapi.Esapi.Authenticator().GetCurrentUser().Roles;
      return this.SearchForRule(map, roles, path).allow;
    }

    private AccessController.Rule SearchForRule(IDictionary map, IList roles, string path)
    {
      string str1 = (string) null;
      try
      {
        str1 = Owasp.Esapi.Esapi.Encoder().Canonicalize(path);
      }
      catch (EncodingException ex)
      {
        AccessController.logger.LogWarning(ILogger_Fields.SECURITY, "Failed to canonicalize input: " + path);
      }
      string str2 = str1;
      while (str2.EndsWith("/"))
        str2 = str2.Substring(0, str2.Length - 1);
      if (str2.IndexOf("..") != -1)
        throw new IntrusionException("Attempt to manipulate access control path", "Attempt to manipulate access control path: " + path);
      string str3 = "";
      int num = str2.LastIndexOf(".");
      if (num != -1)
        str3 = str2.Substring(num + 1);
      AccessController.Rule rule = ((AccessController.Rule) map[(object) str2] ?? (AccessController.Rule) map[(object) (str2 + "/*")]) ?? (AccessController.Rule) map[(object) ("*." + str3)];
      if (rule != null && this.Overlap(rule.roles, roles))
        return rule;
      if (!str2.Contains("/"))
        return this.deny;
      string path1 = str2.Substring(0, str2.LastIndexOf('/'));
      if (path1.Length <= 1)
        return this.deny;
      return this.SearchForRule(map, roles, path1);
    }

    private bool Overlap(IList ruleRoles, IList userRoles)
    {
      if (ruleRoles.Contains((object) "any"))
        return true;
      foreach (string userRole in (IEnumerable) userRoles)
      {
        if (ruleRoles.Contains((object) userRole))
          return true;
      }
      return false;
    }

    private IDictionary LoadRules(FileInfo f)
    {
      IDictionary dictionary = (IDictionary) new Hashtable();
      FileStream fileStream = (FileStream) null;
      try
      {
        fileStream = new FileStream(f.FullName, FileMode.Open, FileAccess.Read);
        string str1;
        while ((str1 = Owasp.Esapi.Esapi.Validator().SafeReadLine((Stream) fileStream, 500)) != null)
        {
          if (str1.Length > 0 && str1[0] != '#')
          {
            AccessController.Rule rule = new AccessController.Rule();
            string[] strArray = str1.Split(new string[1]
            {
              "|"
            }, StringSplitOptions.None);
            rule.path = strArray[0].Trim().Replace("\\", "/");
            rule.roles.Add((object) strArray[1].Trim().ToLower());
            string str2 = strArray[2].Trim();
            rule.allow = str2.ToUpper().Equals("allow".ToUpper());
            if (dictionary.Contains((object) rule.path))
              AccessController.logger.LogWarning(ILogger_Fields.SECURITY, "Problem in access control file. Duplicate rule ignored: " + (object) rule);
            dictionary[(object) rule.path] = (object) rule;
          }
        }
        return dictionary;
      }
      catch (Exception ex)
      {
        AccessController.logger.LogWarning(ILogger_Fields.SECURITY, "Problem in access control file", ex);
      }
      finally
      {
        try
        {
          fileStream?.Close();
        }
        catch (IOException ex)
        {
          AccessController.logger.LogWarning(ILogger_Fields.SECURITY, "Failure closing access control file: " + (object) f, (Exception) ex);
        }
      }
      return dictionary;
    }

    private class Rule
    {
      protected internal string path = "";
      protected internal IList roles = (IList) new ArrayList();
      protected internal bool allow = false;

      public override string ToString()
      {
        return "URL:" + this.path + " | " + this.roles.ToString() + " | " + (this.allow ? "allow" : "deny");
      }
    }
  }
}
