// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.SafeFile
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using Owasp.Esapi.Errors;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Owasp.Esapi
{
  public class SafeFile
  {
    private static readonly long serialVersionUID = 1;
    private readonly string dirBlackList = "([*?<>|])";
    private readonly string fileBlackList = "([\\\\/:*?<>|])";
    private readonly string percents = "(%)([0-9a-fA-F])([0-9a-fA-F])";
    private FileInfo safeFileInfo;

    public SafeFile(string path)
    {
      try
      {
        this.safeFileInfo = new FileInfo(path);
      }
      catch (ArgumentException ex)
      {
        throw new ValidationException("File path was invalid.", "File path caused ArgumentException", (Exception) ex);
      }
      this.DoDirCheck(this.safeFileInfo.DirectoryName);
      this.DoFileCheck(this.safeFileInfo.Name);
    }

    public SafeFile(Uri uri)
    {
      this.safeFileInfo = new FileInfo(new Uri(uri.ToString()).LocalPath);
      this.DoDirCheck(this.safeFileInfo.DirectoryName);
      this.DoFileCheck(this.safeFileInfo.Name);
    }

    private void DoDirCheck(string path)
    {
      string matches1 = this.GetMatches(path, this.dirBlackList);
      if (matches1 != null)
        throw new ValidationException("Invalid directory", "Directory path (" + path + ") contains illegal character: " + matches1);
      string matches2 = this.GetMatches(path, this.percents);
      if (matches2 != null)
        throw new ValidationException("Invalid directory", "Directory path (" + path + ") contains encoded characters: " + matches2);
      int num = this.ContainsUnprintableCharacters(path);
      if (num != -1)
        throw new ValidationException("Invalid directory", "Directory path (" + path + ") contains unprintable character: " + (object) num);
    }

    private void DoFileCheck(string path)
    {
      string matches1 = this.GetMatches(path, this.fileBlackList);
      if (matches1 != null)
        throw new ValidationException("Invalid file", "File path (" + path + ") contains illegal character: " + matches1);
      string matches2 = this.GetMatches(path, this.percents);
      if (matches2 != null)
        throw new ValidationException("Invalid file", "File path (" + path + ") contains encoded characters: " + matches2);
      int num = this.ContainsUnprintableCharacters(path);
      if (num != -1)
        throw new ValidationException("Invalid file", "File path (" + path + ") contains unprintable character: " + (object) num);
    }

    private int ContainsUnprintableCharacters(string s)
    {
      char[] charArray = s.ToCharArray();
      for (int index = 0; index < s.Length; ++index)
      {
        char ch = charArray[index];
        if (ch < ' ' || ch > '~')
          return (int) ch;
      }
      return -1;
    }

    public string GetMatches(string text, string regex)
    {
      MatchCollection matchCollection = Regex.Matches(text, regex);
      if (matchCollection.Count == 0)
        return (string) null;
      string str = "";
      foreach (Match match in matchCollection)
      {
        foreach (Group group in match.Groups)
          str += str.Length == 0 ? group.ToString() : ", " + group.ToString();
      }
      return str;
    }

    public FileInfo SafeFileInfo
    {
      get
      {
        return this.safeFileInfo;
      }
    }
  }
}
