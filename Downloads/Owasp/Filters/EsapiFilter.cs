// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Filters.EsapiFilter
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using Owasp.Esapi.Interfaces;
using System;
using System.Web;

namespace Owasp.Esapi.Filters
{
  public class EsapiFilter : IHttpModule
  {
    private static readonly Logger logger = Logger.GetLogger("ESAPIFilter", "ESAPIFilter");
    private static readonly string[] ignore = new string[1]
    {
      "password"
    };

    private void Application_BeginRequest(object source, EventArgs e)
    {
      HttpContext current = HttpContext.Current;
      HttpRequest request = current.Request;
      HttpResponse response = current.Response;
      try
      {
        IHttpUtilities httpUtilities = Owasp.Esapi.Esapi.HttpUtilities();
        httpUtilities.checkCSRFToken();
        httpUtilities.SetNoCacheHeaders();
        httpUtilities.SafeSetContentType();
      }
      catch (Exception ex)
      {
        EsapiFilter.logger.LogSpecial("Security error in ESAPI Filter", ex);
        response.Output.WriteLine("<H1>Security Error</H1>");
      }
    }

    public void Dispose()
    {
    }

    public void Init(HttpApplication context)
    {
      context.BeginRequest += new EventHandler(this.Application_BeginRequest);
    }
  }
}
