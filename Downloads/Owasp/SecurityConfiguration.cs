// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.SecurityConfiguration
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using log4net.Core;
using Owasp.Esapi.Interfaces;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;

namespace Owasp.Esapi
{
  public class SecurityConfiguration : ISecurityConfiguration
  {
    private static readonly Logger logger = Logger.GetLogger("ESAPI", nameof (SecurityConfiguration));
    private NameValueCollection properties = new NameValueCollection();
    private IDictionary regexMap = (IDictionary) null;
    public const string RESOURCE_DIRECTORY = "Owasp.Esapi.resources";
    private const string ALLOWED_LOGIN_ATTEMPTS = "AllowedLoginAttempts";
    private const string MASTER_PASSWORD = "MasterPassword";
    private const string MASTER_SALT = "MasterSalt";
    private const string VALID_EXTENSIONS = "ValidExtensions";
    private const string MAX_UPLOAD_FILE_BYTES = "MaxUploadFileBytes";
    private const string USERNAME_PARAMETER_NAME = "UsernameParameterName";
    private const string PASSWORD_PARAMETER_NAME = "PasswordParameterName";
    private const string MAX_OLD_PASSWORD_HASHES = "MaxOldPasswordHashes";
    private const string ENCRYPTION_ALGORITHM = "EncryptionAlgorithm";
    private const string HASH_ALGORITHM = "HashAlgorithm";
    private const string CHARACTER_ENCODING = "CharacterEncoding";
    private const string RANDOM_ALGORITHM = "RandomAlgorithm";
    private const string DIGITAL_SIGNATURE_ALGORITHM = "DigitalSignatureAlgorithm";
    private const string RESPONSE_CONTENT_TYPE = "ResponseContentType";
    private const string REMEMBER_TOKEN_DURATION = "RememberTokenDuration";
    private const string LOG_LEVEL = "LogLevel";
    private const string REQUIRE_SECURE_CHANNEL = "RequireSecureChannel";
    protected const int MAX_REDIRECT_LOCATION = 1000;
    protected const int MAX_FILE_NAME_LENGTH = 1000;
    private static string resourceDirectory;
    private static DateTime lastModified;

    public string MasterPassword
    {
      get
      {
        return this.properties.Get(nameof (MasterPassword));
      }
    }

    public FileInfo Keystore
    {
      get
      {
        return new FileInfo(this.ResourceDirectory.FullName + "\\keystore");
      }
    }

    public FileInfo ResourceDirectory
    {
      get
      {
        return new FileInfo(SecurityConfiguration.resourceDirectory);
      }
      set
      {
        SecurityConfiguration.resourceDirectory = value.FullName;
      }
    }

    public byte[] MasterSalt
    {
      get
      {
        return Convert.FromBase64String(this.properties.Get(nameof (MasterSalt)));
      }
    }

    public IList AllowedFileExtensions
    {
      get
      {
        return (IList) new ArrayList((ICollection) Regex.Split(this.properties["ValidExtensions"] == null ? ".zip,.pdf,.tar,.gz,.xls,.properties,.txt,.xml" : this.properties["ValidExtensions"], ","));
      }
    }

    public int AllowedFileUploadSize
    {
      get
      {
        return int.Parse(this.properties["MaxUploadFileBytes"] == null ? "50000" : this.properties["MaxUploadFileBytes"]);
      }
    }

    public string PasswordParameterName
    {
      get
      {
        return this.properties[nameof (PasswordParameterName)] == null ? "password" : this.properties[nameof (PasswordParameterName)];
      }
    }

    public string UsernameParameterName
    {
      get
      {
        return this.properties[nameof (UsernameParameterName)] == null ? "username" : this.properties[nameof (UsernameParameterName)];
      }
    }

    public string EncryptionAlgorithm
    {
      get
      {
        return this.properties[nameof (EncryptionAlgorithm)] == null ? "PBEWithMD5AndDES/CBC/PKCS5Padding" : this.properties[nameof (EncryptionAlgorithm)];
      }
    }

    public string HashAlgorithm
    {
      get
      {
        return this.properties[nameof (HashAlgorithm)] == null ? "SHA-512" : this.properties[nameof (HashAlgorithm)];
      }
    }

    public string CharacterEncoding
    {
      get
      {
        return this.properties[nameof (CharacterEncoding)] == null ? "UTF-8" : this.properties[nameof (CharacterEncoding)];
      }
    }

    public string DigitalSignatureAlgorithm
    {
      get
      {
        return this.properties[nameof (DigitalSignatureAlgorithm)] == null ? "" : this.properties[nameof (DigitalSignatureAlgorithm)];
      }
    }

    public string RandomAlgorithm
    {
      get
      {
        return this.properties[nameof (RandomAlgorithm)] == null ? "" : this.properties[nameof (RandomAlgorithm)];
      }
    }

    public int AllowedLoginAttempts
    {
      get
      {
        return int.Parse(this.properties[nameof (AllowedLoginAttempts)] == null ? "5" : this.properties[nameof (AllowedLoginAttempts)]);
      }
    }

    public int MaxOldPasswordHashes
    {
      get
      {
        return int.Parse(this.properties[nameof (MaxOldPasswordHashes)] == null ? "12" : this.properties[nameof (MaxOldPasswordHashes)]);
      }
    }

    public Level LogLevel
    {
      get
      {
        string str = this.properties.Get(nameof (LogLevel));
        if (str.ToUpper().Equals("TRACE".ToUpper()))
          return (Level) Level.Trace;
        if (str.ToUpper().Equals("ERROR".ToUpper()))
          return (Level) Level.Error;
        if (str.ToUpper().Equals("SEVERE".ToUpper()))
          return (Level) Level.Severe;
        if (str.ToUpper().Equals("WARNING".ToUpper()))
          return (Level) Level.Warn;
        if (str.ToUpper().Equals("SUCCESS".ToUpper()))
          return (Level) Level.Info;
        if (str.ToUpper().Equals("DEBUG".ToUpper()))
          return (Level) Level.Debug;
        if (str.ToUpper().Equals("NONE".ToUpper()))
          return (Level) Level.Off;
        return (Level) Level.All;
      }
    }

    public string ResponseContentType
    {
      get
      {
        return this.properties[nameof (ResponseContentType)] == null ? "text/html; charset=UTF-8" : this.properties[nameof (ResponseContentType)];
      }
    }

    public long RememberTokenDuration
    {
      get
      {
        return 86400000L * (long) int.Parse(this.properties[nameof (RememberTokenDuration)] == null ? "14" : this.properties[nameof (RememberTokenDuration)]);
      }
    }

    public IEnumerator ValidationPatternNames
    {
      get
      {
        ArrayList arrayList = new ArrayList();
        foreach (string property in (NameObjectCollectionBase) this.properties)
        {
          if (property.StartsWith("Validator."))
            arrayList.Add((object) property.Substring(property.IndexOf('.') + 1));
        }
        return arrayList.GetEnumerator();
      }
    }

    public bool LogEncodingRequired
    {
      get
      {
        string str = this.properties.Get(nameof (LogEncodingRequired));
        return str == null || !str.ToUpper().Equals("false".ToUpper());
      }
    }

    public bool RequireSecureChannel
    {
      get
      {
        return Convert.ToBoolean(this.properties[nameof (RequireSecureChannel)]);
      }
    }

    public SecurityConfiguration()
    {
      this.LoadConfiguration();
    }

    private void LoadConfiguration()
    {
      try
      {
        this.properties = (NameValueCollection) ConfigurationManager.GetSection("esapi");
        SecurityConfiguration.resourceDirectory = this.properties.Get("ResourceDirectory");
        SecurityConfiguration.logger.LogSpecial("Loaded ESAPI properties from espai/authentication", (Exception) null);
      }
      catch (Exception ex)
      {
        SecurityConfiguration.logger.LogSpecial("Can't load ESAPI properties from espai/authentication", ex);
      }
      SecurityConfiguration.logger.LogSpecial("  ========Master Configuration========", (Exception) null);
      foreach (string property in (NameObjectCollectionBase) this.properties)
        SecurityConfiguration.logger.LogSpecial("  |   " + property + "=" + this.properties[property], (Exception) null);
      SecurityConfiguration.logger.LogSpecial("  ========Master Configuration========", (Exception) null);
      this.regexMap = (IDictionary) new Hashtable();
      IEnumerator validationPatternNames = this.ValidationPatternNames;
      while (validationPatternNames.MoveNext())
      {
        string current = (string) validationPatternNames.Current;
        Regex validationPattern = this.GetValidationPattern(current);
        if (current != null && validationPattern != null)
          this.regexMap[(object) current] = (object) validationPattern;
      }
    }

    public Threshold GetQuota(string eventName)
    {
      int count = 0;
      string s1 = this.properties.Get(eventName + ".count");
      if (s1 != null)
        count = int.Parse(s1);
      int num = 0;
      string s2 = this.properties.Get(eventName + ".interval");
      if (s2 != null)
        num = int.Parse(s2);
      IList actions = (IList) new ArrayList();
      string input = this.properties.Get(eventName + ".actions");
      if (input != null)
        actions = (IList) new ArrayList((ICollection) Regex.Split(input, ","));
      return new Threshold(eventName, count, (long) num, actions);
    }

    public Regex GetValidationPattern(string key)
    {
      string pattern = this.properties.Get("Validator." + key);
      if (pattern == null)
        return (Regex) null;
      return new Regex(pattern);
    }
  }
}
