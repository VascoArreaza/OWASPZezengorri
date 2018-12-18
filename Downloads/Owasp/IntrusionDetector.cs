// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.IntrusionDetector
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using Owasp.Esapi.Errors;
using Owasp.Esapi.Interfaces;
using System;
using System.Collections;

namespace Owasp.Esapi
{
  public class IntrusionDetector : IIntrusionDetector
  {
    private static readonly Logger logger = Logger.GetLogger("ESAPI", nameof (IntrusionDetector));

    public void AddException(Exception e)
    {
      if (e is EnterpriseSecurityException)
        IntrusionDetector.logger.LogWarning(ILogger_Fields.SECURITY, ((EnterpriseSecurityException) e).LogMessage, e);
      else
        IntrusionDetector.logger.LogWarning(ILogger_Fields.SECURITY, e.Message, e);
      User currentUser = (User) Owasp.Esapi.Esapi.Authenticator().GetCurrentUser();
      string fullName = e.GetType().FullName;
      if (e is IntrusionException)
        return;
      try
      {
        currentUser.AddSecurityEvent(fullName);
      }
      catch (IntrusionException ex)
      {
        Threshold quota = Owasp.Esapi.Esapi.SecurityConfiguration().GetQuota(fullName);
        foreach (string action in (IEnumerable) quota.Actions)
        {
          string message = "User exceeded quota of " + (object) quota.Count + " per " + (object) quota.Interval + " seconds for event " + fullName + ". Taking actions " + quota.Actions.ToString();
          this.TakeSecurityAction(action, message);
        }
      }
    }

    public virtual void AddEvent(string eventName)
    {
      IntrusionDetector.logger.LogWarning(ILogger_Fields.SECURITY, "Security event " + eventName + " received");
      User currentUser = (User) Owasp.Esapi.Esapi.Authenticator().GetCurrentUser();
      try
      {
        currentUser.AddSecurityEvent("event." + eventName);
      }
      catch (IntrusionException ex)
      {
        Threshold quota = Owasp.Esapi.Esapi.SecurityConfiguration().GetQuota("event." + eventName);
        foreach (string action in (IEnumerable) quota.Actions)
        {
          string message = "User exceeded quota of " + (object) quota.Count + " per " + (object) quota.Interval + " seconds for event " + eventName + ". Taking actions " + quota.Actions.ToString();
          this.TakeSecurityAction(action, message);
        }
      }
    }

    private void TakeSecurityAction(string action, string message)
    {
      if (action.Equals("log"))
        IntrusionDetector.logger.LogCritical(ILogger_Fields.SECURITY, "INTRUSION - " + message);
      if (action.Equals("disable"))
        Owasp.Esapi.Esapi.Authenticator().GetCurrentUser().Disable();
      if (!action.Equals("logout"))
        return;
      ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).Logout();
    }
  }
}
