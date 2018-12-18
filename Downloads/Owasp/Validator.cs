// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Validator
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using Owasp.Esapi.Errors;
using Owasp.Esapi.Interfaces;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Owasp.Esapi
{
    public class Validator : IValidator
  {
    private static readonly int MAX_CREDIT_CARD_LENGTH = 19;
    private static readonly int MAX_PARAMETER_NAME_LENGTH = 100;
    private static readonly int MAX_PARAMETER_VALUE_LENGTH = 10000;
    private static readonly Logger logger = Logger.GetLogger("Esapi", nameof (Validator));

        public Validator()
        {
        }

        public string GetValidInput(string context, string type, string input, int maxLength, bool allowNull)
    {
      try
      {
        context = Owasp.Esapi.Esapi.Encoder().Canonicalize(context);
        string input1 = Owasp.Esapi.Esapi.Encoder().Canonicalize(input);
        if (Validator.IsEmpty(input1))
        {
          if (allowNull)
            return (string) null;
          throw new ValidationException(context + " is required", type + " (" + context + ") input to validate was null");
        }
        if (input1.Length > maxLength)
          throw new ValidationException(context + " can not exceed " + (object) maxLength + " characters", type + " (" + context + "=" + input + ") input exceeds maximum allowed length of " + (object) maxLength + " by " + (object) (input1.Length - maxLength) + " characters");
        if (type == null)
          throw new ValidationException(context + " is invalid", type + " (" + context + ") type to validate against was null");
        Regex validationPattern = ((SecurityConfiguration) Owasp.Esapi.Esapi.SecurityConfiguration()).GetValidationPattern(type);
        if (validationPattern == null)
          throw new ValidationException(context + " is invalid", type + " (" + context + ") type to validate against not configured in ESAPI.properties");
        if (!validationPattern.Match(input1).Success)
          throw new ValidationException(context + " is invalid", type + " (" + context + "=" + input + ") input did not match type definition " + validationPattern.ToString());
        return input1;
      }
      catch (EncodingException ex)
      {
        throw new ValidationException(context + " is invalid", "Error canonicalizing user input", (Exception) ex);
      }
    }

    public bool IsValidInput(string context, string type, string data, int maxLength, bool allowNull)
    {
      try
      {
        this.GetValidInput(context, type, data, maxLength, allowNull);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public DateTime GetValidDate(string context, string input, DateTimeFormatInfo format, bool allowNull)
    {
      try
      {
        return DateTime.Parse(input, (IFormatProvider) format);
      }
      catch (Exception ex)
      {
        throw new ValidationException("Invalid date", "Problem parsing date (" + context + "=" + input + ") ", ex);
      }
    }

    public bool IsValidDate(string context, string input, DateTimeFormatInfo format, bool allowNull)
    {
      try
      {
        this.GetValidDate(context, input, format, allowNull);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public string GetValidCreditCard(string context, string input, bool allowNull)
    {
      if (Validator.IsEmpty(input))
      {
        if (allowNull)
          return (string) null;
        throw new ValidationException(context + " is required", "(" + context + ") input is required");
      }
      string validInput = this.GetValidInput(context, "CreditCard", input, Validator.MAX_CREDIT_CARD_LENGTH, allowNull);
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < validInput.Length; ++index)
      {
        char c = validInput[index];
        if (char.IsDigit(c))
          stringBuilder.Append(c);
      }
      int num1 = 0;
      bool flag = false;
      for (int startIndex = stringBuilder.Length - 1; startIndex >= 0; --startIndex)
      {
        int num2 = int.Parse(stringBuilder.ToString(startIndex, 1));
        int num3;
        if (flag)
        {
          num3 = num2 * 2;
          if (num3 > 9)
            num3 -= 9;
        }
        else
          num3 = num2;
        num1 += num3;
        flag = !flag;
      }
      if (num1 % 10 != 0)
        throw new ValidationException(context + " invalid", context + " invalid");
      return validInput;
    }

    public bool IsValidCreditCard(string context, string input, bool allowNull)
    {
      try
      {
        this.GetValidCreditCard(context, input, allowNull);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public string GetValidDirectoryPath(string context, string input, bool allowNull)
    {
      string input1 = "";
      try
      {
        if (Validator.IsEmpty(input))
        {
          if (allowNull)
            return (string) null;
          throw new ValidationException(context + " is required", "(" + context + ") input is required");
        }
        input1 = Owasp.Esapi.Esapi.Encoder().Canonicalize(input);
        Regex validationPattern = ((SecurityConfiguration) Owasp.Esapi.Esapi.SecurityConfiguration()).GetValidationPattern("DirectoryName");
        if (!validationPattern.Match(input1).Success)
          throw new ValidationException(context + " is an invalid directory name", "Attempt to use a directory name (" + input1 + ") that violates the global rule in ESAPI.properties (" + validationPattern.ToString() + ")");
        string str1 = new DirectoryInfo(input).FullName.Replace("\\", "/");
        string lower1 = str1.ToLower();
        if (lower1.Length >= 2 && lower1[0] >= 'a' && lower1[0] <= 'z' && lower1[1] == ':')
          str1 = str1.Substring(2);
        string str2 = input1.Replace("\\", "/");
        string lower2 = str2.ToLower();
        if (lower2.Length >= 2 && lower2[0] >= 'a' && lower2[0] <= 'z' && lower2[1] == ':')
          str2 = str2.Substring(2);
        if (!str2.Equals(str1.ToLower()))
          throw new ValidationException(context + " is an invalid directory name", "The input path does not match the canonical path (" + input1 + ")");
      }
      catch (IOException ex)
      {
        throw new ValidationException(context + " is an invalid directory name", "Attempt to use a directory name (" + input1 + ") that does not exist");
      }
      catch (EncodingException ex)
      {
        throw new IntrusionException(context + " is an invalid directory name", "Exception during directory validation", (Exception) ex);
      }
      return input1;
    }

    public bool IsValidDirectoryPath(string context, string input, bool allowNull)
    {
      try
      {
        this.GetValidDirectoryPath(context, input, allowNull);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public string GetValidFileName(string context, string input, bool allowNull)
    {
      if (Validator.IsEmpty(input))
      {
        if (allowNull)
          return (string) null;
        throw new ValidationException(context + " is required", "(" + context + ") input is required");
      }
      string str1;
      try
      {
        str1 = Owasp.Esapi.Esapi.Encoder().Canonicalize(input);
        Regex validationPattern = ((SecurityConfiguration) Owasp.Esapi.Esapi.SecurityConfiguration()).GetValidationPattern("FileName");
        if (!validationPattern.Match(str1).Success)
          throw new ValidationException(context + " is an invalid filename", "Attempt to use a filename (" + str1 + ") that violates the global rule in ESAPI.properties (" + validationPattern.ToString() + ")");
        string fullName = new FileInfo(str1).FullName;
        string str2 = fullName.Substring(fullName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        if (!input.Equals(str2))
          throw new ValidationException(context + " is an invalid filename", "Invalid filename (" + str1 + ") doesn't match canonical path (" + str2 + ") and could be an injection attack");
      }
      catch (IOException ex)
      {
        throw new IntrusionException(context + " is an invalid filename", "Exception during filename validation", (Exception) ex);
      }
      catch (EncodingException ex)
      {
        throw new IntrusionException(context + " is an invalid filename", "Exception during filename validation", (Exception) ex);
      }
      foreach (string allowedFileExtension in (IEnumerable) Owasp.Esapi.Esapi.SecurityConfiguration().AllowedFileExtensions)
      {
        if (input.ToLower().EndsWith(allowedFileExtension.ToLower()))
          return str1;
      }
      throw new IntrusionException(context + " is an invalid filename", "Extension does not exist in EASPI.getAllowedFileExtensions list");
    }

    public bool IsValidFileName(string context, string input, bool allowNull)
    {
      try
      {
        this.GetValidFileName(context, input, allowNull);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public bool IsValidNumber(string context, string input, long minValue, long maxValue, bool allowNull)
    {
      try
      {
        this.GetValidNumber(context, input, minValue, maxValue, allowNull);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public double GetValidNumber(string context, string input, long minValue, long maxValue, bool allowNull)
    {
      double minValue1 = Convert.ToDouble(minValue);
      double maxValue1 = Convert.ToDouble(maxValue);
      return this.GetValidDouble(context, input, minValue1, maxValue1, allowNull);
    }

    public bool IsValidDouble(string context, string input, double minValue, double maxValue, bool allowNull)
    {
      try
      {
        this.GetValidDouble(context, input, minValue, maxValue, allowNull);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public double GetValidDouble(string context, string input, double minValue, double maxValue, bool allowNull)
    {
      if (minValue > maxValue)
        throw new ValidationException("maxValue (" + (object) maxValue + ") must be greater than minValue (" + (object) minValue + ") for " + context, "maxValue (" + (object) maxValue + ") must be greater than minValue (" + (object) minValue + ") for " + context);
      if (Validator.IsEmpty(input))
      {
        if (allowNull)
          return 0.0;
        throw new ValidationException(context + " is required", context + " is required");
      }
      try
      {
        double d = double.Parse(input);
        if (double.IsInfinity(d))
          throw new ValidationException(context + " is an invalid number", "Number (" + input + ") is infinite");
        if (double.IsNaN(d))
          throw new ValidationException(context + " is an invalid number", "Number (" + input + ") is not a number");
        if (d < minValue)
          throw new ValidationException(context + " must be between " + (object) minValue + " and " + (object) maxValue, "Invalid number (" + input + "). Number must be between " + (object) minValue + " and " + (object) maxValue);
        if (d > maxValue)
          throw new ValidationException(context + " must be between " + (object) minValue + " and " + (object) maxValue, "Invalid number (" + input + "). Number must be between " + (object) minValue + " and " + (object) maxValue);
        return d;
      }
      catch (FormatException ex)
      {
        throw new ValidationException(context + " is an invalid number", "Invalid number format (" + input + ")", (Exception) ex);
      }
    }

    public bool IsValidInteger(string context, string input, int minValue, int maxValue, bool allowNull)
    {
      try
      {
        this.GetValidInteger(context, input, minValue, maxValue, allowNull);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public int GetValidInteger(string context, string input, int minValue, int maxValue, bool allowNull)
    {
      if (minValue > maxValue)
        throw new ValidationException("maxValue (" + (object) maxValue + ") must be greater than minValue (" + (object) minValue + ") for " + context, "maxValue (" + (object) maxValue + ") must be greater than minValue (" + (object) minValue + ") for " + context);
      if (Validator.IsEmpty(input))
      {
        if (allowNull)
          return 0;
        throw new ValidationException(context + " is required", "Input is required");
      }
      try
      {
        int num = int.Parse(input);
        if (num < minValue)
          throw new ValidationException(context + " must be between " + (object) minValue + " and " + (object) maxValue, "Invalid Integer. Integer must be between " + (object) minValue + " and " + (object) maxValue);
        if (num > maxValue)
          throw new ValidationException(context + " must be between " + (object) minValue + " and " + (object) maxValue, "Invalid Integer. Integer must be between " + (object) minValue + " and " + (object) maxValue);
        return Convert.ToInt32(num);
      }
      catch (FormatException ex)
      {
        throw new ValidationException(context + " is an invalid integer", "Invalid Integer", (Exception) ex);
      }
    }

    public bool IsValidFileContents(string context, byte[] input, int maxBytes, bool allowNull)
    {
      try
      {
        this.GetValidFileContents(context, input, maxBytes, allowNull);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public byte[] GetValidFileContents(string context, byte[] input, int maxBytes, bool allowNull)
    {
      if (Validator.IsEmpty(input))
      {
        if (allowNull)
          return (byte[]) null;
        throw new ValidationException(context + " is required", "(" + context + ") input is required");
      }
      long allowedFileUploadSize = (long) Owasp.Esapi.Esapi.SecurityConfiguration().AllowedFileUploadSize;
      if ((long) input.Length > allowedFileUploadSize)
        throw new ValidationException(context + " can not exceed " + (object) allowedFileUploadSize + " bytes", "Exceeded ESAPI max length");
      if (input.Length > maxBytes)
        throw new ValidationException(context + " can not exceed " + (object) maxBytes + " bytes", "Exceeded maxBytes (" + (object) input.Length + ")");
      return input;
    }

    public bool IsValidFileUpload(string context, string directorypath, string filename, byte[] content, int maxBytes, bool allowNull)
    {
      return this.IsValidFileName(context, filename, allowNull) && this.IsValidDirectoryPath(context, directorypath, allowNull) && this.IsValidFileContents(context, content, maxBytes, allowNull);
    }

    public void AssertValidFileUpload(string context, string directorypath, string filename, byte[] content, int maxBytes, bool allowNull)
    {
      this.GetValidFileName(context, filename, allowNull);
      this.GetValidDirectoryPath(context, directorypath, allowNull);
      this.GetValidFileContents(context, content, maxBytes, allowNull);
    }

    public bool IsValidHttpRequest()
    {
      try
      {
        this.AssertValidHttpRequest();
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public bool IsValidHttpRequest(HttpRequest request)
    {
      try
      {
        //this.AssertIsValidHttpRequest();
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public void AssertValidHttpRequest()
    {
      this.AssertIsValidHttpRequest(((Authenticator) Owasp.Esapi.Esapi.Authenticator()).Context.Request);
    }

    public void AssertIsValidHttpRequest(HttpRequest request)
    {
      if (request == null)
        throw new ValidationException("Invalid HTTPRequest", "HTTPRequest is null");
      ArrayList arrayList = new ArrayList();
      arrayList.AddRange((ICollection) new ArrayList((ICollection) request.Form.AllKeys));
      arrayList.AddRange((ICollection) new ArrayList((ICollection) request.QueryString.AllKeys));
      foreach (string index in arrayList)
      {
        this.GetValidInput("http", "HTTPParameterName", index, Validator.MAX_PARAMETER_NAME_LENGTH, false);
        string input = request.Params[index];
        this.GetValidInput(index, "HTTPParameterValue", input, Validator.MAX_PARAMETER_VALUE_LENGTH, true);
      }
      if (request.Cookies != null)
      {
        foreach (string cookie in (NameObjectCollectionBase) request.Cookies)
        {
          string input = request.Cookies[cookie].Value;
          this.GetValidInput("http", "HTTPCookieName", cookie, Validator.MAX_PARAMETER_NAME_LENGTH, true);
          this.GetValidInput(cookie, "HTTPCookieValue", input, Validator.MAX_PARAMETER_VALUE_LENGTH, true);
        }
      }
      foreach (string header1 in (NameObjectCollectionBase) request.Headers)
      {
        if (header1 != null && !header1.ToUpper().Equals("Cookie".ToUpper()))
        {
          this.GetValidInput("http", "HTTPHeaderName", header1, Validator.MAX_PARAMETER_NAME_LENGTH, true);
          foreach (string key in request.Headers.Keys)
          {
            this.GetValidInput("http", "HTTPHeaderName", key, Validator.MAX_PARAMETER_NAME_LENGTH, true);
            string header2 = request.Params[key];
            this.GetValidInput(header1, "HTTPHeaderValue", header2, Validator.MAX_PARAMETER_VALUE_LENGTH, true);
          }
        }
      }
    }

    public bool IsValidListItem(string context, string input, IList list)
    {
      try
      {
        this.GetValidListItem(context, input, list);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public string GetValidListItem(string context, string input, IList list)
    {
      if (list.Contains((object) input))
        return input;
      throw new ValidationException(context + " does not exist in list", "Item (" + input + ") does not exist in List " + context);
    }

    public bool IsValidHttpRequestParameterSet(string context, IList required, IList optional)
    {
      try
      {
        this.AssertIsValidHttpRequestParameterSet(context, required, optional);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public void AssertIsValidHttpRequestParameterSet(string context, IList required, IList optional)
    {
      ArrayList arrayList1 = new ArrayList((ICollection) ((Authenticator) Owasp.Esapi.Esapi.Authenticator()).Context.Request.Params.Keys);
      ArrayList arrayList2 = (ArrayList) ((ArrayList) required).Clone();
      foreach (object obj in arrayList1)
        arrayList2.Remove(obj);
      if (arrayList2.Count > 0)
        throw new ValidationException(context + " is an invalid parameter set", "Required element missing");
      ArrayList arrayList3 = (ArrayList) arrayList1.Clone();
      foreach (object obj in (IEnumerable) required)
        arrayList3.Remove(obj);
      foreach (object obj in (IEnumerable) optional)
        arrayList3.Remove(obj);
      if (arrayList3.Count > 0)
        throw new ValidationException(context + " is an invalid parameter set", "Parameters other than optional + required parameters are present");
    }

    public bool IsValidPrintable(string context, byte[] input, int maxLength, bool allowNull)
    {
      try
      {
        this.GetValidPrintable(context, input, maxLength, allowNull);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public byte[] GetValidPrintable(string context, byte[] input, int maxLength, bool allowNull)
    {
      if (Validator.IsEmpty(input))
      {
        if (allowNull)
          return (byte[]) null;
        throw new ValidationException(context + " is required", "Input is required");
      }
      if (input.Length > maxLength)
        throw new ValidationException(context + " can not exceed " + (object) maxLength + " bytes", "Invalid Input. Input exceeded maxLength");
      for (int index = 0; index < input.Length; ++index)
      {
        if (input[index] < (byte) 33 || input[index] > (byte) 126)
          throw new ValidationException(context + " is invalid", "Invalid Input. Some characters are not ASCII.");
      }
      return input;
    }

    public bool IsValidPrintable(string context, string input, int maxLength, bool allowNull)
    {
      try
      {
        this.GetValidPrintable(context, input, maxLength, allowNull);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public string GetValidPrintable(string context, string input, int maxLength, bool allowNull)
    {
      string s = "";
      try
      {
        s = Owasp.Esapi.Esapi.Encoder().Canonicalize(input);
        ASCIIEncoding asciiEncoding = new ASCIIEncoding();
        this.GetValidPrintable(context, asciiEncoding.GetBytes(s), maxLength, allowNull);
      }
      catch (EncodingException ex)
      {
        Validator.logger.LogError(ILogger_Fields.SECURITY, "Could not canonicalize user input", (Exception) ex);
      }
      return s;
    }

    public bool IsValidRedirectLocation(string context, string input, bool allowNull)
    {
      return Owasp.Esapi.Esapi.Validator().IsValidInput(context, "Redirect", input, 512, allowNull);
    }

    public string GetValidRedirectLocation(string context, string input, bool allowNull)
    {
      return Owasp.Esapi.Esapi.Validator().GetValidInput(context, "Redirect", input, 512, allowNull);
    }

    public bool IsValidSafeHtml(string context, string input, int maxLength, bool allowNull)
    {
      throw new NotSupportedException();
    }

    public string GetValidSafeHtml(string context, string input, int maxLength, bool allowNull)
    {
      throw new NotSupportedException();
    }

    public string SafeReadLine(Stream inStream, int max)
    {
      if (max <= 0)
        throw new ValidationAvailabilityException("Invalid input", "Must read a positive number of bytes from the stream");
      StringBuilder stringBuilder = new StringBuilder();
      int num1 = 0;
      try
      {
        while (true)
        {
          int num2 = inStream.ReadByte();
          int num3;
          switch (num2)
          {
            case -1:
              goto label_4;
            case 10:
              num3 = 0;
              break;
            default:
              num3 = num2 != 13 ? 1 : 0;
              break;
          }
          if (num3 != 0)
          {
            ++num1;
            if (num1 <= max)
              stringBuilder.Append((char) num2);
            else
              goto label_10;
          }
          else
            goto label_13;
        }
label_4:
        if (stringBuilder.Length == 0)
          return (string) null;
        goto label_13;
label_10:
        throw new ValidationAvailabilityException("Invalid input", "Read more than maximum characters allowed (" + (object) max + ")");
label_13:
        return stringBuilder.ToString();
      }
      catch (IOException ex)
      {
        throw new ValidationAvailabilityException("Invalid input", "Problem reading from input stream", (Exception) ex);
      }
    }

    private static bool IsEmpty(string input)
    {
      return input == null || input.Trim().Length == 0;
    }

    private static bool IsEmpty(byte[] input)
    {
      return input == null || input.Length == 0;
    }

        bool IValidator.IsValidInput(string context, string type, string data, int maxLength, bool allowNull)
        {
            throw new NotImplementedException();
        }

        string IValidator.GetValidInput(string context, string type, string input, int maxLength, bool allowNull)
        {
            throw new NotImplementedException();
        }

        bool IValidator.IsValidDate(string context, string input, DateTimeFormatInfo format, bool allowNull)
        {
            throw new NotImplementedException();
        }

        DateTime IValidator.GetValidDate(string context, string input, DateTimeFormatInfo format, bool allowNull)
        {
            throw new NotImplementedException();
        }

        bool IValidator.IsValidCreditCard(string context, string data, bool allowNull)
        {
            throw new NotImplementedException();
        }

        string IValidator.GetValidCreditCard(string context, string input, bool allowNull)
        {
            throw new NotImplementedException();
        }

        bool IValidator.IsValidDirectoryPath(string context, string dirpath, bool allowNull)
        {
            throw new NotImplementedException();
        }

        string IValidator.GetValidDirectoryPath(string context, string dirpath, bool allowNull)
        {
            throw new NotImplementedException();
        }

        bool IValidator.IsValidDouble(string context, string input, double minValue, double maxValue, bool allowNull)
        {
            throw new NotImplementedException();
        }

        double IValidator.GetValidDouble(string context, string input, double minValue, double maxValue, bool allowNull)
        {
            throw new NotImplementedException();
        }

        bool IValidator.IsValidFileContents(string context, byte[] input, int maxBytes, bool allowNull)
        {
            throw new NotImplementedException();
        }

        byte[] IValidator.GetValidFileContents(string context, byte[] input, int maxBytes, bool allowNull)
        {
            throw new NotImplementedException();
        }

        bool IValidator.IsValidFileName(string context, string input, bool allowNull)
        {
            throw new NotImplementedException();
        }

        string IValidator.GetValidFileName(string context, string input, bool allowNull)
        {
            throw new NotImplementedException();
        }

        bool IValidator.IsValidFileUpload(string context, string filepath, string filename, byte[] content, int maxBytes, bool allowNull)
        {
            throw new NotImplementedException();
        }

        void IValidator.AssertValidFileUpload(string context, string filepath, string filename, byte[] content, int maxBytes, bool allowNull)
        {
            throw new NotImplementedException();
        }

        bool IValidator.IsValidHttpRequest()
        {
            throw new NotImplementedException();
        }

        bool IValidator.IsValidHttpRequest(IHttpRequest request)
        {
            throw new NotImplementedException();
        }

        void IValidator.AssertValidHttpRequest()
        {
            throw new NotImplementedException();
        }

        bool IValidator.IsValidInteger(string context, string input, int minValue, int maxValue, bool allowNull)
        {
            throw new NotImplementedException();
        }

        int IValidator.GetValidInteger(string context, string input, int minValue, int maxValue, bool allowNull)
        {
            throw new NotImplementedException();
        }

        bool IValidator.IsValidListItem(string context, string listValue, IList list)
        {
            throw new NotImplementedException();
        }

        string IValidator.GetValidListItem(string context, string input, IList list)
        {
            throw new NotImplementedException();
        }

        bool IValidator.IsValidNumber(string context, string input, long minValue, long maxValue, bool allowNull)
        {
            throw new NotImplementedException();
        }

        double IValidator.GetValidNumber(string context, string input, long minValue, long maxValue, bool allowNull)
        {
            throw new NotImplementedException();
        }

        bool IValidator.IsValidHttpRequestParameterSet(string context, IList requiredNames, IList optionalNames)
        {
            throw new NotImplementedException();
        }

        void IValidator.AssertIsValidHttpRequestParameterSet(string context, IList requiredNames, IList optionalNames)
        {
            throw new NotImplementedException();
        }

        bool IValidator.IsValidPrintable(string context, byte[] input, int maxLength, bool allowNull)
        {
            throw new NotImplementedException();
        }

        byte[] IValidator.GetValidPrintable(string context, byte[] input, int maxLength, bool allowNull)
        {
            throw new NotImplementedException();
        }

        bool IValidator.IsValidPrintable(string context, string input, int maxLength, bool allowNull)
        {
            throw new NotImplementedException();
        }

        string IValidator.GetValidPrintable(string context, string input, int maxLength, bool allowNull)
        {
            throw new NotImplementedException();
        }

        bool IValidator.IsValidRedirectLocation(string context, string location, bool allowNull)
        {
            throw new NotImplementedException();
        }

        string IValidator.GetValidRedirectLocation(string context, string input, bool allowNull)
        {
            throw new NotImplementedException();
        }

        bool IValidator.IsValidSafeHtml(string context, string input, int maxLength, bool allowNull)
        {
            throw new NotImplementedException();
        }

        string IValidator.GetValidSafeHtml(string context, string input, int maxLength, bool allowNull)
        {
            throw new NotImplementedException();
        }

        string IValidator.SafeReadLine(Stream inStream, int max)
        {
            throw new NotImplementedException();
        }
    }

    public interface IHttpRequest
    {
    }
}
