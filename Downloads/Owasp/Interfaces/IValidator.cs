// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Interfaces.IValidator
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll


using System;
using System.Collections;
using System.Globalization;
using System.IO;

namespace Owasp.Esapi.Interfaces
{
  public interface IValidator
  {
    bool IsValidInput(string context, string type, string data, int maxLength, bool allowNull);

    string GetValidInput(string context, string type, string input, int maxLength, bool allowNull);

    bool IsValidDate(string context, string input, DateTimeFormatInfo format, bool allowNull);

    DateTime GetValidDate(string context, string input, DateTimeFormatInfo format, bool allowNull);

    bool IsValidCreditCard(string context, string data, bool allowNull);

    string GetValidCreditCard(string context, string input, bool allowNull);

    bool IsValidDirectoryPath(string context, string dirpath, bool allowNull);

    string GetValidDirectoryPath(string context, string dirpath, bool allowNull);

    bool IsValidDouble(string context, string input, double minValue, double maxValue, bool allowNull);

    double GetValidDouble(string context, string input, double minValue, double maxValue, bool allowNull);

    bool IsValidFileContents(string context, byte[] input, int maxBytes, bool allowNull);

    byte[] GetValidFileContents(string context, byte[] input, int maxBytes, bool allowNull);

    bool IsValidFileName(string context, string input, bool allowNull);

    string GetValidFileName(string context, string input, bool allowNull);

    bool IsValidFileUpload(string context, string filepath, string filename, byte[] content, int maxBytes, bool allowNull);

    void AssertValidFileUpload(string context, string filepath, string filename, byte[] content, int maxBytes, bool allowNull);

    bool IsValidHttpRequest();

    bool IsValidHttpRequest(IHttpRequest request);

    void AssertValidHttpRequest();

    bool IsValidInteger(string context, string input, int minValue, int maxValue, bool allowNull);

    int GetValidInteger(string context, string input, int minValue, int maxValue, bool allowNull);

    bool IsValidListItem(string context, string listValue, IList list);

    string GetValidListItem(string context, string input, IList list);

    bool IsValidNumber(string context, string input, long minValue, long maxValue, bool allowNull);

    double GetValidNumber(string context, string input, long minValue, long maxValue, bool allowNull);

    bool IsValidHttpRequestParameterSet(string context, IList requiredNames, IList optionalNames);

    void AssertIsValidHttpRequestParameterSet(string context, IList requiredNames, IList optionalNames);

    bool IsValidPrintable(string context, byte[] input, int maxLength, bool allowNull);

    byte[] GetValidPrintable(string context, byte[] input, int maxLength, bool allowNull);

    bool IsValidPrintable(string context, string input, int maxLength, bool allowNull);

    string GetValidPrintable(string context, string input, int maxLength, bool allowNull);

    bool IsValidRedirectLocation(string context, string location, bool allowNull);

    string GetValidRedirectLocation(string context, string input, bool allowNull);

    bool IsValidSafeHtml(string context, string input, int maxLength, bool allowNull);

    string GetValidSafeHtml(string context, string input, int maxLength, bool allowNull);

    string SafeReadLine(Stream inStream, int max);
  }
}
