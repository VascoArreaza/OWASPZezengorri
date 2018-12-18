// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Randomizer
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using Owasp.Esapi.Errors;
using Owasp.Esapi.Interfaces;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Owasp.Esapi
{
  public class Randomizer : IRandomizer
  {
    private static readonly Logger logger = Logger.GetLogger("Esapi", nameof (Randomizer));
    private RandomNumberGenerator randomNumberGenerator = (RandomNumberGenerator) null;

    public Randomizer()
    {
      string randomAlgorithm = Owasp.Esapi.Esapi.SecurityConfiguration().RandomAlgorithm;
      try
      {
        this.randomNumberGenerator = RandomNumberGenerator.Create();
      }
      catch (Exception ex)
      {
        EncryptionException encryptionException = new EncryptionException("Error creating randomizer", "Can't find random algorithm " + randomAlgorithm, ex);
      }
    }

    public bool RandomBoolean
    {
      get
      {
        byte[] data = new byte[1];
        this.randomNumberGenerator.GetBytes(data);
        return data[0] >= (byte) 128;
      }
    }

    public string RandomGUID
    {
      get
      {
        StringBuilder stringBuilder1 = new StringBuilder();
        try
        {
          IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
          stringBuilder1.Append(hostEntry.AddressList[0].ToString());
        }
        catch (Exception ex)
        {
          stringBuilder1.Append("0.0.0.0");
        }
        stringBuilder1.Append(":");
        stringBuilder1.Append(DateTime.Now.Ticks);
        stringBuilder1.Append(":");
        stringBuilder1.Append(this.GetRandomString(20, Encoder.CHAR_ALPHANUMERICS));
        string input = Owasp.Esapi.Esapi.Encryptor().Hash(stringBuilder1.ToString(), "salt");
        byte[] numArray = (byte[]) null;
        try
        {
          numArray = Owasp.Esapi.Esapi.Encoder().DecodeFromBase64(input);
        }
        catch (IOException ex)
        {
          Randomizer.logger.LogCritical(ILogger_Fields.SECURITY, "Problem decoding hash while creating GUID: " + input);
        }
        StringBuilder stringBuilder2 = new StringBuilder();
        for (int index = 0; index < numArray.Length; ++index)
        {
          int num = (int) numArray[index] & (int) byte.MaxValue;
          if (num < 16)
            stringBuilder2.Append('0');
          stringBuilder2.Append(Convert.ToString(num, 16));
        }
        string upper = stringBuilder2.ToString().ToUpper();
        StringBuilder stringBuilder3 = new StringBuilder();
        stringBuilder3.Append(upper.Substring(0, 8));
        stringBuilder3.Append("-");
        stringBuilder3.Append(upper.Substring(8, 4));
        stringBuilder3.Append("-");
        stringBuilder3.Append(upper.Substring(12, 4));
        stringBuilder3.Append("-");
        stringBuilder3.Append(upper.Substring(16, 4));
        stringBuilder3.Append("-");
        stringBuilder3.Append(upper.Substring(20));
        return stringBuilder3.ToString();
      }
    }

    public string GetRandomString(int length, char[] characterSet)
    {
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < length; ++index)
      {
        int randomInteger = this.GetRandomInteger(0, characterSet.GetLength(0) - 1);
        stringBuilder.Append(characterSet[randomInteger]);
      }
      return stringBuilder.ToString();
    }

    public int GetRandomInteger(int min, int max)
    {
      int num1 = max - min;
      byte[] data = new byte[4];
      this.randomNumberGenerator.GetBytes(data);
      double num2 = (double) BitConverter.ToUInt32(data, 0) / (double) uint.MaxValue;
      return Convert.ToInt32(Math.Round((double) num1 * num2)) + min;
    }

    public float GetRandomReal(float min, float max)
    {
      return (float) (this.GetRandomInteger(0, int.MaxValue) / int.MaxValue) * (max - min) + min;
    }

    public string GetRandomFilename(string extension)
    {
      return this.GetRandomString(12, Encoder.CHAR_ALPHANUMERICS) + "." + extension;
    }

    public static char[] Union(char[] c1, char[] c2)
    {
      StringBuilder sb = new StringBuilder();
      for (int index = 0; index < c1.Length; ++index)
      {
        if (!Randomizer.Contains(sb, c1[index]))
          sb.Append(c1[index]);
      }
      for (int index = 0; index < c2.Length; ++index)
      {
        if (!Randomizer.Contains(sb, c2[index]))
          sb.Append(c2[index]);
      }
      char[] array = new char[sb.Length];
      int index1 = 0;
      int index2 = 0;
      while (index1 < sb.Length)
      {
        array[index2] = sb[index1];
        ++index1;
        ++index2;
      }
      Array.Sort<char>(array);
      return array;
    }

    public static bool Contains(StringBuilder sb, char c)
    {
      for (int index = 0; index < sb.Length; ++index)
      {
        if ((int) sb[index] == (int) c)
          return true;
      }
      return false;
    }
  }
}
