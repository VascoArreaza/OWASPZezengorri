// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Interfaces.IEncoder
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

namespace Owasp.Esapi.Interfaces
{
  public interface IEncoder
  {
    string Canonicalize(string input);

    string Normalize(string input);

    string EncodeForHtml(string input);

    string EncodeForHtmlAttribute(string input);

    string EncodeForJavascript(string input);

    string EncodeForVbScript(string input);

    string EncodeForSql(string input);

    string EncodeForLdap(string input);

    string EncodeForDn(string input);

    string EncodeForXPath(string input);

    string EncodeForXml(string input);

    string EncodeForXmlAttribute(string input);

    string EncodeForUrl(string input);

    string DecodeFromUrl(string input);

    string EncodeForBase64(byte[] input, bool wrap);

    byte[] DecodeFromBase64(string input);
  }
}
