// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Authenticator
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using Owasp.Esapi.Errors;
using Owasp.Esapi.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace Owasp.Esapi
{
  public  class Authenticator : IAuthenticator
  {
    internal IUser anonymous = (IUser) new User(nameof (anonymous), nameof (anonymous));
    private FileInfo userDB = (FileInfo) null;
    private long checkInterval = 60000;
    private long lastModified = 0;
    private long lastChecked = 0;
    private IDictionary userMap = (IDictionary) new Hashtable();
    private readonly int MAX_ACCOUNT_NAME_LENGTH = 250;
    private static readonly Logger logger = Logger.GetLogger("ESAPI", nameof (Authenticator));
    protected internal const string USER = "ESAPIUserContextKey";
    private HttpContext context;

    public HttpContext Context
    {
      get
      {
        return context;
      }
      set
      {
         context = value;
      }
    }

    public HttpRequest CurrentRequest
    {
      get
      {
        return Context == null ? (HttpRequest) null : Context.Request;
      }
    }

    public HttpResponse CurrentResponse
    {
      get
      {
        return Context == null ? (HttpResponse) null : Context.Response;
      }
    }

    public HttpSessionState CurrentSession
    {
      get
      {
        try
        {
          return Context == null ? (HttpSessionState)null : Context.Session;
        }
        catch (NullReferenceException ex)
        {
          return (HttpSessionState) null;
        }
      }
    }

    [STAThread]
    public static void Main(string[] args)
    {
      if (args.Length != 3)
      {
        Console.Out.WriteLine("Usage: Authenticator accountname password role");
      }
      else
      {
        Authenticator authenticator = new Authenticator();
        string lower = args[0].ToLower();
        string password = args[1];
        string role = args[2];
        User user = (User) authenticator.GetUser(args[0]);
        if (user == null)
        {
          user = new User();
          user.AccountName = lower;
          authenticator.userMap[(object) lower] = (object) user;
          Authenticator.logger.LogCritical(ILogger_Fields.SECURITY, "New user created: " + lower);
        }
        string hash = authenticator.HashPassword(password, lower);
        user.SetHashedPassword(hash);
        user.AddRole(role);
        user.Enable();
        user.Unlock();
        authenticator.SaveUsers();
        long lastModified = authenticator.lastModified;
        long ticks = authenticator.userDB.LastWriteTime.Ticks;
        Console.Out.WriteLine("User account " + user.AccountName + " updated");
      }
    }

    public IUser CreateUser(string accountName, string password1, string password2)
    {
      lock (Authenticator.USER)
      {
        LoadUsersIfNecessary();
        if (accountName == null)
          throw new AuthenticationAccountsException("Account creation failed", "Attempt to create user with null accountName");
        if (userMap.Contains((object) accountName.ToLower()))
          throw new AuthenticationAccountsException("Account creation failed", "Duplicate user creation denied for " + accountName);
        IUser user = (IUser) new User(accountName, password1, password2);
        userMap[(object) accountName.ToLower()] = (object) user;
        Authenticator.logger.LogCritical(ILogger_Fields.SECURITY, "New user created: " + accountName);
        SaveUsers();
        return user;
      }
    }

    public bool Exists(string accountName)
    {
      return GetUser(accountName) != null;
    }

    public virtual string GenerateStrongPassword()
    {
      return GenerateStrongPassword("");
    }

    private string GenerateStrongPassword(string oldPassword)
    {
      IRandomizer randomizer = Owasp.Esapi.Esapi.Randomizer();
      string newPassword = "";
      int num = 10;
      for (int index = 0; index < num; ++index)
      {
        try
        {
          newPassword = randomizer.GetRandomString(8, Encoder.CHAR_PASSWORD);
          VerifyPasswordStrength(newPassword, oldPassword);
          return newPassword;
        }
        catch (AuthenticationException ex)
        {
          Authenticator.logger.LogDebug(ILogger_Fields.SECURITY, "Password generator created weak password: " + newPassword + ". Regenerating.", (Exception) ex);
        }
      }
      Authenticator.logger.LogCritical(ILogger_Fields.SECURITY, "Strong password generation failed after  " + (object) num + " attempts");
      return (string) null;
    }

    public string GenerateStrongPassword(string oldPassword, IUser user)
    {
      string strongPassword = GenerateStrongPassword(oldPassword);
      if (strongPassword != null)
        Authenticator.logger.LogCritical(ILogger_Fields.SECURITY, "Generated strong password for " + user.AccountName);
      return strongPassword;
    }

    public IUser GetCurrentUser()
    {
      if (Context == null)
        return anonymous;
      return (IUser) Context.Items[(object) "ESAPIUserContextKey"] ?? anonymous;
    }

    public IUser GetUser(string accountName)
    {
      lock (Authenticator.USER)
      {
        LoadUsersIfNecessary();
        return (IUser) userMap[(object) accountName.ToLower()];
      }
    }

    public IUser GetUserFromSession(IHttpRequest request)
    {
      HttpSessionState currentSession = CurrentSession;
      if (currentSession != null)
      {
        string accountName = (string) currentSession.SessionID;
        if (accountName != null)
        {
          IUser user = GetUser(accountName);
          if (user != null)
          {
            SetCurrentUser(user);
            return user;
          }
        }
      }
      return (IUser) null;
    }

    public string HashPassword(string password, string accountName)
    {
      string lower = accountName.ToLower();
      return Owasp.Esapi.Esapi.Encryptor().Hash(password, lower);
    }

    public void RemoveUser(string accountName)
    {
      lock ((Authenticator.USER))
      {
        LoadUsersIfNecessary();
        if (GetUser(accountName) == null)
          throw new AuthenticationAccountsException("Remove user failed", "Can't remove invalid accountName " + accountName);
        userMap.Remove((object) accountName.ToLower());
        SaveUsers();
        Authenticator.logger.LogCritical(ILogger_Fields.SECURITY, "User " + accountName + " removed");
      }
    }

    public void VerifyPasswordStrength(string newPassword, string oldPassword)
    {
      string str1 = oldPassword == null ? "" : oldPassword;
      int length = str1.Length;
      for (int startIndex = 0; startIndex < length - 2; ++startIndex)
      {
        string str2 = str1.Substring(startIndex, startIndex + 3 - startIndex);
        if (newPassword.IndexOf(str2) > -1)
          throw new AuthenticationCredentialsException("Invalid password", "New password cannot contain pieces of old password");
      }
      int num = 0;
      for (int index = 0; index < newPassword.Length; ++index)
      {
        if (Array.BinarySearch<char>(Encoder.CHAR_LOWERS, newPassword[index]) > 0)
        {
          ++num;
          break;
        }
      }
      for (int index = 0; index < newPassword.Length; ++index)
      {
        if (Array.BinarySearch<char>(Encoder.CHAR_UPPERS, newPassword[index]) > 0)
        {
          ++num;
          break;
        }
      }
      for (int index = 0; index < newPassword.Length; ++index)
      {
        if (Array.BinarySearch<char>(Encoder.CHAR_DIGITS, newPassword[index]) > 0)
        {
          ++num;
          break;
        }
      }
      for (int index = 0; index < newPassword.Length; ++index)
      {
        if (Array.BinarySearch<char>(Encoder.CHAR_SPECIALS, newPassword[index]) > 0)
        {
          ++num;
          break;
        }
      }
      if (newPassword.Length * num < 16)
        throw new AuthenticationCredentialsException("Invalid password", "New password is not long and complex enough");
    }

    public IUser Login()
    {
      HttpRequest request = Context.Request;
      HttpResponse response = Context.Response;
      if (Owasp.Esapi.Esapi.SecurityConfiguration().RequireSecureChannel && !Owasp.Esapi.Esapi.HttpUtilities().SecureChannel)
        throw new AuthenticationCredentialsException("Session exposed", "Authentication attempt made over non-SSL connection. Check web.xml and server configuration");
      User user = (User) GetUserFromSession(request);
      if (user != null)
      {
        user.SetLastHostAddress(request.UserHostAddress);
        user.SetFirstRequest(false);
      }
      else
      {
        user.SetFirstRequest(true);
      }
      if (user.Anonymous)
        throw new AuthenticationLoginException("Login failed", "Anonymous user cannot be set to current user");
      if (!user.Enabled)
      {
        DateTime now = DateTime.Now;
        user.SetLastFailedLoginTime(now);
        throw new AuthenticationLoginException("Login failed", "Disabled user cannot be set to current user: " + user.AccountName);
      }
      if (user.Locked)
      {
        DateTime now = DateTime.Now;
        user.SetLastFailedLoginTime(now);
        throw new AuthenticationLoginException("Login failed", "Locked user cannot be set to current user: " + user.AccountName);
      }
      if (user.Expired)
      {
        DateTime now = DateTime.Now;
        user.SetLastFailedLoginTime(now);
        throw new AuthenticationLoginException("Login failed", "Expired user cannot be set to current user: " + user.AccountName);
      }
      SetCurrentUser((IUser) user);
      return (IUser) user;
    }

  

    public static T Cast<T>(Object myobj)
    {
        Type objectType = myobj.GetType();
        Type target = typeof(T);
        var x = Activator.CreateInstance(target, false);
        var z = from source in objectType.GetMembers().ToList()
                where source.MemberType == MemberTypes.Property
                select source;
        var d = from source in target.GetMembers().ToList()
                where source.MemberType == MemberTypes.Property
                select source;
        List<MemberInfo> members = d.Where(memberInfo => d.Select(c => c.Name)
            .ToList().Contains(memberInfo.Name)).ToList();
        PropertyInfo propertyInfo;
        object value;
        foreach (var memberInfo in members)
        {
            propertyInfo = typeof(T).GetProperty(memberInfo.Name);
            value = myobj.GetType().GetProperty(memberInfo.Name).GetValue(myobj, null);

            propertyInfo.SetValue(x, value, null);
        }
        return (T)x;
    }

    public IUser GetUserFromSession(HttpRequest request)
    {
        return GetUserFromSession((Owasp.Esapi.IHttpRequest)request.LogonUserIdentity) ;
    }

        public void Logout()
    {
      GetCurrentUser().Logout();
    }

    public void SetCurrentUser(IUser user)
    {
      Context.Items[(object) "ESAPIUserContextKey"] = (object) user;
    }

    public void VerifyAccountNameStrength(string newAccountName)
    {
      if (newAccountName == null)
        throw new AuthenticationCredentialsException("Invalid account name", "Attempt to create account with a null account name");
      if (!Owasp.Esapi.Esapi.Validator().IsValidInput(nameof (VerifyAccountNameStrength), "AccountName", newAccountName, MAX_ACCOUNT_NAME_LENGTH, false))
        throw new AuthenticationCredentialsException("Invalid account name", "New account name is not valid: " + newAccountName);
    }

    public IList GetUserNames()
    {
      lock (Authenticator.USER)
      {
        LoadUsersIfNecessary();
        return (IList) new ArrayList(userMap.Keys);
      }
    }

    protected internal void LoadUsersIfNecessary()
    {
      string fullName = ((SecurityConfiguration) Owasp.Esapi.Esapi.SecurityConfiguration()).ResourceDirectory.FullName;
      if (userDB == null)
        userDB = new FileInfo(fullName + "\\users.txt");
      long ticks = DateTime.Now.Ticks;
      if (ticks - lastChecked < checkInterval)
        return;
      lastChecked = ticks;
      if (lastModified == userDB.LastWriteTime.Ticks)
        return;
      LoadUsersImmediately();
    }

    public void LoadUsersImmediately()
    {
      lock (Authenticator.USER)
      {
        Authenticator.logger.LogTrace(ILogger_Fields.SECURITY, "Loading users from " + userDB.FullName, (Exception) null);
        StreamReader streamReader = (StreamReader) null;
        try
        {
          Hashtable hashtable = new Hashtable();
          streamReader = new StreamReader(userDB.FullName, Encoding.Default);
          string line;
          while ((line = streamReader.ReadLine()) != null)
          {
            if (line.Length > 0 && line[0] != '#')
            {
              IUser user = (IUser) new User(line);
              if (!user.AccountName.Equals("anonymous"))
              {
                if (hashtable.ContainsKey((object) user.AccountName))
                  Authenticator.logger.LogCritical(ILogger_Fields.SECURITY, "Problem in user file. Skipping duplicate user: " + (object) user, (Exception) null);
                hashtable[(object) user.AccountName] = (object) user;
              }
            }
          }
          userMap = (IDictionary) hashtable;
          lastModified = lastModified;
          Authenticator.logger.LogTrace(ILogger_Fields.SECURITY, "User file reloaded: " + (object) hashtable.Count, (Exception) null);
        }
        catch (Exception ex)
        {
          Authenticator.logger.LogCritical(ILogger_Fields.SECURITY, "Failure loading user file: " + userDB.FullName, ex);
        }
        finally
        {
          try
          {
            streamReader?.Close();
          }
          catch (IOException ex)
          {
            Authenticator.logger.LogCritical(ILogger_Fields.SECURITY, "Failure closing user file: " + userDB.FullName, (Exception) ex);
          }
        }
      }
    }

    public void SaveUsers()
    {
      lock (Authenticator.USER)
      {
        StreamWriter writer = (StreamWriter) null;
        try
        {
          writer = new StreamWriter(userDB.FullName, false, Encoding.Default);
          writer.WriteLine("# This is the user file associated with the ESAPI library from http://www.owasp.org");
          writer.WriteLine("# accountName | hashedPassword | roles | locked | enabled | rememberToken | csrfToken | oldPasswordHashes | lastPasswordChangeTime | lastLoginTime | lastFailedLoginTime | expirationTime | failedLoginCount");
          writer.WriteLine();
          SaveUsers(writer);
          writer.Flush();
          Authenticator.logger.LogCritical(ILogger_Fields.SECURITY, "User file written to disk");
        }
        catch (IOException ex)
        {
          Authenticator.logger.LogCritical(ILogger_Fields.SECURITY, "Problem saving user file " + userDB.FullName, (Exception) ex);
          throw new AuthenticationException("Internal Error", "Problem saving user file " + userDB.FullName, (Exception) ex);
        }
        finally
        {
          if (writer != null)
          {
            writer.Close();
            lastModified = userDB.LastWriteTime.Ticks;
            lastChecked = lastModified;
          }
        }
      }
    }

    internal void SaveUsers(StreamWriter writer)
    {
      lock (Authenticator.USER)
      {
        foreach (string userName in (IEnumerable) GetUserNames())
        {
          User user = (User) GetUser(userName);
          if (user != null && !user.Anonymous)
          {
            writer.WriteLine(user.Save());
          }
          else
          {
            AuthenticationCredentialsException credentialsException = new AuthenticationCredentialsException("Problem saving user", "Skipping save of user " + userName);
          }
        }
        Authenticator.logger.LogTrace(ILogger_Fields.SECURITY, "User file updated", (Exception) null);
      }
    }

    
  }
}
