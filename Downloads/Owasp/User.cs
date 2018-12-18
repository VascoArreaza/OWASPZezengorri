// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.User
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll


using Owasp.Esapi.Errors;
using Owasp.Esapi.Interfaces;
using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Owasp.Esapi
{
  public class User : IUser
  {
    private static readonly Logger logger = Logger.GetLogger("Esapi", nameof (User));
    private bool isFirstRequest = true;
    private string accountName = "";
    private string screenName = "";
    private string hashedPassword = "";
    private IList oldPasswordHashes = (IList) new ArrayList();
    private string rememberToken = "";
    private string csrfToken = "";
    private ArrayList roles = new ArrayList();
    private bool locked = false;
    private bool loggedIn = true;
    private bool enabled = false;
    private string lastHostAddress = "";
    private DateTime lastPasswordChangeTime = DateTime.Now;
    private DateTime lastLoginTime = DateTime.Now;
    private DateTime lastFailedLoginTime = DateTime.Now;
    private DateTime expirationTime = DateTime.MaxValue;
    private int failedLoginCount = 0;
    private IDictionary events = (IDictionary) new Hashtable();
    private const long serialVersionUID = 1;
    private const int MAX_ROLE_LENGTH = 250;

    public string AccountName
    {
      get
      {
        return this.accountName;
      }
      set
      {
        string str = value;
        this.accountName = value.ToLower();
        User.logger.LogCritical(ILogger_Fields.SECURITY, "Account name changed from " + str + " to " + this.AccountName);
      }
    }

    public string CsrfToken
    {
      get
      {
        return this.csrfToken;
      }
    }

    public DateTime ExpirationTime
    {
      get
      {
        return this.expirationTime;
      }
      set
      {
        this.expirationTime = new DateTime(value.Ticks);
        User.logger.LogCritical(ILogger_Fields.SECURITY, "Account expiration time set to " + value.ToString("r") + " for " + this.AccountName);
      }
    }

    public int FailedLoginCount
    {
      get
      {
        return this.failedLoginCount;
      }
    }

    public string RememberToken
    {
      get
      {
        return this.rememberToken;
      }
    }

    public ArrayList Roles
    {
      get
      {
        return ArrayList.ReadOnly(this.roles);
      }
      set
      {
        this.roles = new ArrayList();
        this.AddRoles(value);
        User.logger.LogCritical(ILogger_Fields.SECURITY, "Adding roles " + value.ToString() + " to " + this.AccountName);
      }
    }

    public string ScreenName
    {
      get
      {
        return this.screenName;
      }
      set
      {
        this.screenName = value;
        User.logger.LogCritical(ILogger_Fields.SECURITY, "ScreenName changed to " + value + " for " + this.AccountName);
      }
    }

    public bool Anonymous
    {
      get
      {
        return this.AccountName.Equals("anonymous");
      }
    }

    public bool Enabled
    {
      get
      {
        return this.enabled;
      }
    }

    public bool Expired
    {
      get
      {
        return this.ExpirationTime < DateTime.Now;
      }
    }

    public bool Locked
    {
      get
      {
        return this.locked;
      }
    }

    public bool LoggedIn
    {
      get
      {
        return this.loggedIn;
      }
    }

    protected internal User()
    {
    }

    protected internal User(string line)
    {
      string[] strArray = Regex.Split(line, "\\|");
      this.accountName = strArray[0].Trim().ToLower();
      this.hashedPassword = strArray[1].Trim();
      this.roles = new ArrayList((ICollection) Regex.Split(strArray[2].Trim().ToLower(), " *, *"));
      this.locked = !"unlocked".ToUpper().Equals(strArray[3].Trim().ToUpper());
      this.enabled = nameof (enabled).ToUpper().Equals(strArray[4].Trim().ToUpper());
      this.rememberToken = strArray[5].Trim();
      this.ResetCsrfToken();
      this.oldPasswordHashes = (IList) new ArrayList((ICollection) Regex.Split(strArray[6].Trim(), " *, *"));
      this.lastHostAddress = strArray[7].Trim();
      this.lastPasswordChangeTime = new DateTime(long.Parse(strArray[8].Trim()));
      this.lastLoginTime = new DateTime(long.Parse(strArray[9].Trim()));
      this.lastFailedLoginTime = new DateTime(long.Parse(strArray[10].Trim()));
      this.expirationTime = new DateTime(long.Parse(strArray[11].Trim()));
      this.failedLoginCount = int.Parse(strArray[12].Trim());
    }

    protected internal User(string accountName, string password)
    {
      this.accountName = accountName.ToLower();
    }

    public User(string accountName, string password1, string password2)
    {
      Owasp.Esapi.Esapi.Authenticator().VerifyAccountNameStrength(accountName);
      if (password1 == null)
        throw new AuthenticationCredentialsException("Invalid account name", "Attempt to create account " + accountName + " with a null password");
      Owasp.Esapi.Esapi.Authenticator().VerifyPasswordStrength(password1, (string) null);
      if (!password1.Equals(password2))
        throw new AuthenticationCredentialsException("Passwords do not match", "Passwords for " + accountName + " do not match");
      this.accountName = accountName.ToLower();
      try
      {
        this.SetHashedPassword(Owasp.Esapi.Esapi.Encryptor().Hash(password1, this.accountName));
      }
      catch (EncryptionException ex)
      {
        throw new AuthenticationException("Internal error", "Error hashing password for " + this.accountName, (Exception) ex);
      }
      this.expirationTime = new DateTime(DateTime.Now.Ticks + 7776000000L);
      User.logger.LogCritical(ILogger_Fields.SECURITY, "Account created successfully: " + accountName);
    }

    public void AddRole(string role)
    {
      string lower = role.ToLower();
      if (!Owasp.Esapi.Esapi.Validator().IsValidInput("addRole", "RoleName", lower, 250, false))
        throw new AuthenticationAccountsException("Add role failed", "Attempt to add invalid role " + lower + " to " + this.AccountName);
      this.roles.Add((object) lower);
      User.logger.LogCritical(ILogger_Fields.SECURITY, "Role " + lower + " added to " + this.AccountName);
    }

    public void AddRoles(ArrayList newRoles)
    {
      foreach (string newRole in newRoles)
        this.AddRole(newRole);
    }

    public void AddSecurityEvent(string eventName)
    {
      User.Event @event = (User.Event) this.events[(object) eventName];
      if (@event == null)
      {
        @event = new User.Event(eventName);
        this.events[(object) eventName] = (object) @event;
      }
      Threshold quota = Owasp.Esapi.Esapi.SecurityConfiguration().GetQuota(eventName);
      if (quota.Count <= 0)
        return;
      @event.Increment(quota.Count, quota.Interval);
    }

    protected internal void ChangePassword(string newPassword1, string newPassword2)
    {
      this.SetLastPasswordChangeTime(DateTime.Now);
      this.SetHashedPassword(Owasp.Esapi.Esapi.Authenticator().HashPassword(newPassword1, this.AccountName));
      User.logger.LogCritical(ILogger_Fields.SECURITY, "Password changed for user: " + this.AccountName);
    }

    public void ChangePassword(string oldPassword, string newPassword1, string newPassword2)
    {
      if (!this.hashedPassword.Equals(Owasp.Esapi.Esapi.Authenticator().HashPassword(oldPassword, this.AccountName)))
        throw new AuthenticationCredentialsException("Password change failed", "Authentication failed for password chanage on user: " + this.AccountName);
      if (newPassword1 == null || newPassword2 == null || !newPassword1.Equals(newPassword2))
        throw new AuthenticationCredentialsException("Password change failed", "Passwords do not match for password change on user: " + this.AccountName);
      Owasp.Esapi.Esapi.Authenticator().VerifyPasswordStrength(newPassword1, oldPassword);
      this.SetLastPasswordChangeTime(DateTime.Now);
      string hash = Owasp.Esapi.Esapi.Authenticator().HashPassword(newPassword1, this.accountName);
      if (this.oldPasswordHashes.Contains((object) hash))
        throw new AuthenticationCredentialsException("Password change failed", "Password change matches a recent password for user: " + this.AccountName);
      this.SetHashedPassword(hash);
      User.logger.LogCritical(ILogger_Fields.SECURITY, "Password changed for user: " + this.AccountName);
    }

    public void Disable()
    {
      this.enabled = false;
      User.logger.LogSpecial("Account disabled: " + this.AccountName, (Exception) null);
    }

    protected internal string Dump(ICollection c)
    {
      StringBuilder stringBuilder = new StringBuilder();
      IEnumerator enumerator = c.GetEnumerator();
      while (enumerator.MoveNext())
      {
        string current = (string) enumerator.Current;
        stringBuilder.Append(current);
        if (enumerator.MoveNext())
          stringBuilder.Append(",");
      }
      return stringBuilder.ToString();
    }

    public void Enable()
    {
      this.enabled = true;
      User.logger.LogSpecial("Account enabled: " + this.AccountName, (Exception) null);
    }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      if (obj == null || !this.GetType().Equals(obj.GetType()))
        return false;
      return this.accountName.Equals(((User) obj).accountName);
    }

    public DateTime GetLastFailedLoginTime()
    {
      return this.lastFailedLoginTime;
    }

    public string GetLastHostAddress()
    {
      HttpRequest currentRequest = ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentRequest;
      if (currentRequest != null)
        this.SetLastHostAddress(currentRequest.UserHostAddress);
      return this.lastHostAddress;
    }

    public DateTime GetLastLoginTime()
    {
      return this.lastLoginTime;
    }

    public DateTime GetLastPasswordChangeTime()
    {
      return this.lastPasswordChangeTime;
    }

    public override int GetHashCode()
    {
      return this.accountName.GetHashCode();
    }

    public void IncrementFailedLoginCount()
    {
      ++this.failedLoginCount;
    }

    public bool IsInRole(string role)
    {
      return this.roles.Contains((object) role.ToLower());
    }

    public bool IsSessionAbsoluteTimeout()
    {
      throw new NotImplementedException();
    }

    public bool IsSessionTimeout()
    {
      return DateTime.Now > new DateTime(DateTime.Now.Ticks + 1200000L);
    }

    public void Lock()
    {
      this.locked = true;
      User.logger.LogCritical(ILogger_Fields.SECURITY, "Account locked: " + this.AccountName);
    }

    public void LoginWithPassword(string password)
    {
      if (password == null || password.Equals(""))
      {
        this.SetLastFailedLoginTime(DateTime.Now);
        throw new AuthenticationLoginException("Login failed", "Missing password: " + this.accountName);
      }
      if (!this.Enabled)
      {
        this.SetLastFailedLoginTime(DateTime.Now);
        throw new AuthenticationLoginException("Login failed", "Disabled user attempt to login: " + this.accountName);
      }
      if (this.Locked)
      {
        this.SetLastFailedLoginTime(DateTime.Now);
        throw new AuthenticationLoginException("Login failed", "Locked user attempt to login: " + this.accountName);
      }
      if (this.Expired)
      {
        this.SetLastFailedLoginTime(DateTime.Now);
        throw new AuthenticationLoginException("Login failed", "Expired user attempt to login: " + this.accountName);
      }
      if (!this.Anonymous)
        this.Logout();
      try
      {
        if (!this.VerifyPassword(password))
          throw new AuthenticationLoginException("Login failed", "Login attempt as " + this.AccountName + " failed");
        this.loggedIn = true;
        ((HttpUtilities) Owasp.Esapi.Esapi.HttpUtilities()).ChangeSessionIdentifier().Add("ESAPIUserContextKey", (object) this.AccountName);
        Owasp.Esapi.Esapi.Authenticator().SetCurrentUser((IUser) this);
        this.SetLastLoginTime(DateTime.Now);
        this.SetLastHostAddress(((Authenticator) Owasp.Esapi.Esapi.Authenticator()).CurrentRequest.UserHostAddress);
        User.logger.LogTrace(ILogger_Fields.SECURITY, "User logged in: " + this.accountName);
      }
      catch (EncryptionException ex)
      {
        throw new AuthenticationException("Internal error", "Error verifying password for " + this.accountName, (Exception) ex);
      }
    }

    public void Logout()
    {
      Authenticator authenticator = (Authenticator) Owasp.Esapi.Esapi.Authenticator();
      if (authenticator.GetCurrentUser().Anonymous)
        return;
      HttpRequest currentRequest = authenticator.CurrentRequest;
      authenticator.Context.Session?.Abandon();
      Owasp.Esapi.Esapi.HttpUtilities().KillCookie("ASPSESSIONID");
      this.loggedIn = false;
      User.logger.LogSuccess(ILogger_Fields.SECURITY, "Logout successful");
      authenticator.SetCurrentUser(authenticator.anonymous);
    }

    public void RemoveRole(string role)
    {
      this.roles.Remove((object) role.ToLower());
      User.logger.LogTrace(ILogger_Fields.SECURITY, "Role " + role + " removed from " + this.AccountName);
    }

    public string ResetCsrfToken()
    {
      this.csrfToken = Owasp.Esapi.Esapi.Randomizer().GetRandomString(8, Encoder.CHAR_ALPHANUMERICS);
      return this.CsrfToken;
    }

    public string ResetPassword()
    {
      string strongPassword = Owasp.Esapi.Esapi.Authenticator().GenerateStrongPassword();
      this.ChangePassword(strongPassword, strongPassword);
      return strongPassword;
    }

    public string ResetRememberToken()
    {
      this.rememberToken = Owasp.Esapi.Esapi.Randomizer().GetRandomString(20, Encoder.CHAR_ALPHANUMERICS);
      User.logger.LogTrace(ILogger_Fields.SECURITY, "New remember token generated for: " + this.AccountName);
      return this.rememberToken;
    }

    protected internal string Save()
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(this.accountName);
      stringBuilder.Append(" | ");
      stringBuilder.Append(this.GetHashedPassword());
      stringBuilder.Append(" | ");
      stringBuilder.Append(this.Dump((ICollection) this.Roles));
      stringBuilder.Append(" | ");
      stringBuilder.Append(this.Locked ? "locked" : "unlocked");
      stringBuilder.Append(" | ");
      stringBuilder.Append(this.Enabled ? "enabled" : "disabled");
      stringBuilder.Append(" | ");
      stringBuilder.Append(this.RememberToken);
      stringBuilder.Append(" | ");
      stringBuilder.Append(this.Dump((ICollection) this.oldPasswordHashes));
      stringBuilder.Append(" | ");
      stringBuilder.Append(this.GetLastHostAddress());
      stringBuilder.Append(" | ");
      stringBuilder.Append(this.GetLastPasswordChangeTime().Ticks);
      stringBuilder.Append(" | ");
      stringBuilder.Append(this.GetLastLoginTime().Ticks);
      stringBuilder.Append(" | ");
      stringBuilder.Append(this.GetLastFailedLoginTime().Ticks);
      stringBuilder.Append(" | ");
      stringBuilder.Append(this.ExpirationTime.Ticks);
      stringBuilder.Append(" | ");
      stringBuilder.Append(this.FailedLoginCount);
      return stringBuilder.ToString();
    }

    public string GetHashedPassword()
    {
      return this.hashedPassword;
    }

    internal void SetHashedPassword(string hash)
    {
      this.oldPasswordHashes.Add((object) this.hashedPassword);
      if (this.oldPasswordHashes.Count > Owasp.Esapi.Esapi.SecurityConfiguration().MaxOldPasswordHashes)
        this.oldPasswordHashes.RemoveAt(0);
      this.hashedPassword = hash;
      User.logger.LogCritical(ILogger_Fields.SECURITY, "New hashed password stored for " + this.AccountName);
    }

    protected internal void SetLastFailedLoginTime(DateTime lastFailedLoginTime)
    {
      this.lastFailedLoginTime = lastFailedLoginTime;
      User.logger.LogCritical(ILogger_Fields.SECURITY, "Set last failed login time to " + lastFailedLoginTime.ToString("r") + " for " + this.AccountName);
    }

    protected internal void SetLastHostAddress(string remoteHost)
    {
      if (this.lastHostAddress.Equals("") || this.lastHostAddress.Equals(remoteHost))
        return;
      AuthenticationHostException authenticationHostException = new AuthenticationHostException("Host change", "User session just jumped from " + this.lastHostAddress + " to " + remoteHost);
      this.lastHostAddress = remoteHost;
    }

    protected internal void SetLastLoginTime(DateTime lastLoginTime)
    {
      this.lastLoginTime = lastLoginTime;
      User.logger.LogCritical(ILogger_Fields.SECURITY, "Set last successful login time to " + lastLoginTime.ToString("r") + " for " + this.AccountName);
    }

    protected internal void SetLastPasswordChangeTime(DateTime lastPasswordChangeTime)
    {
      this.lastPasswordChangeTime = lastPasswordChangeTime;
      User.logger.LogCritical(ILogger_Fields.SECURITY, "Set last password change time to " + lastPasswordChangeTime.ToString("r") + " for " + this.AccountName);
    }

    public override string ToString()
    {
      return "USER:" + this.accountName;
    }

    public void Unlock()
    {
      this.locked = false;
      User.logger.LogSpecial("Account unlocked: " + this.AccountName, (Exception) null);
    }

    public bool VerifyPassword(string password)
    {
      if (Owasp.Esapi.Esapi.Authenticator().HashPassword(password, this.accountName).Equals(this.hashedPassword))
      {
        this.SetLastLoginTime(DateTime.Now);
        this.failedLoginCount = 0;
        User.logger.LogCritical(ILogger_Fields.SECURITY, "Password verified for " + this.AccountName);
        return true;
      }
      User.logger.LogCritical(ILogger_Fields.SECURITY, "Password verification failed for " + this.AccountName);
      this.SetLastFailedLoginTime(DateTime.Now);
      this.IncrementFailedLoginCount();
      if (this.FailedLoginCount >= Owasp.Esapi.Esapi.SecurityConfiguration().AllowedLoginAttempts)
        this.Lock();
      return false;
    }

    protected internal void SetFirstRequest(bool b)
    {
      this.isFirstRequest = b;
    }

    public bool IsFirstRequest()
    {
      return this.isFirstRequest;
    }

    private class Event
    {
      public ArrayList times = new ArrayList();
      public long count = 0;
      public string key;

      public Event(string key)
      {
        this.key = key;
      }

      public void Increment(int count, long interval)
      {
        DateTime now = DateTime.Now;
        this.times.Insert(0, (object) now);
        while (this.times.Count > count)
          this.times.RemoveAt(this.times.Count - 1);
        if (this.times.Count != count)
          return;
        long ticks = ((DateTime) this.times[count - 1]).Ticks;
        if (now.Ticks - ticks < interval * 1000L)
          throw new IntrusionException();
      }
    }
  }
}
