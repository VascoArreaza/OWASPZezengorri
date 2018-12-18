// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.EncryptedProperties
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using Owasp.Esapi.Errors;
using Owasp.Esapi.Interfaces;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Owasp.Esapi
{
  [Serializable]
  public class EncryptedProperties : IEncryptedProperties, IXmlSerializable
  {
    private static readonly Logger logger = Logger.GetLogger("ESAPI", "Encrypted Properties");
    private NameValueCollection properties = new NameValueCollection();

    public string GetProperty(string key)
    {
      lock (this)
      {
        try
        {
          return Owasp.Esapi.Esapi.Encryptor().Decrypt(this.properties.Get(key));
        }
        catch (Exception ex)
        {
          throw new EncryptionException("Property retrieval failure", "Couldn't decrypt property", ex);
        }
      }
    }

    public virtual string SetProperty(string key, string value)
    {
      lock (this)
      {
        try
        {
          object property = (object) this.properties[key];
          this.properties[key] = Owasp.Esapi.Esapi.Encryptor().Encrypt(value);
          return (string) property;
        }
        catch (Exception ex)
        {
          throw new EncryptionException("Property setting failure", "Couldn't encrypt property", ex);
        }
      }
    }

    public virtual IList KeySet()
    {
      return (IList) new ArrayList((ICollection) this.properties.Keys);
    }

    XmlSchema IXmlSerializable.GetSchema()
    {
      return (XmlSchema) null;
    }

    void IXmlSerializable.ReadXml(XmlReader reader)
    {
      this.properties = new NameValueCollection();
      while (reader.Read())
      {
        if (reader.Name.Equals("property"))
        {
          reader.MoveToFirstAttribute();
          this.properties.Add(reader.Name, reader.Value);
        }
      }
    }

    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
      foreach (string key in this.properties.Keys)
      {
        writer.WriteStartElement("property");
        string property = this.properties[key];
        writer.WriteAttributeString(key, property);
        writer.WriteEndElement();
      }
    }

    public void Load(Stream inStream)
    {
      try
      {
        this.properties = ((EncryptedProperties) new XmlSerializer(typeof (EncryptedProperties)).Deserialize((TextReader) new StreamReader(inStream))).properties;
        EncryptedProperties.logger.LogTrace(ILogger_Fields.SECURITY, "Encrypted properties loaded successfully");
      }
      catch (Exception ex)
      {
        EncryptedProperties.logger.LogError(ILogger_Fields.SECURITY, "Encrypted properties could not be loaded successfully", ex);
      }
      finally
      {
        inStream.Close();
      }
    }

    public virtual void Store(Stream outStream, string comments)
    {
      try
      {
        new XmlSerializer(typeof (EncryptedProperties)).Serialize((TextWriter) new StreamWriter(outStream), (object) this);
        EncryptedProperties.logger.LogTrace(ILogger_Fields.SECURITY, "Encrypted properties stored successfully");
      }
      catch
      {
      }
      finally
      {
        outStream.Close();
      }
    }

    [STAThread]
    public static void Main(string[] args)
    {
      FileInfo fileInfo = new FileInfo(args[0]);
      Logger.GetLogger(nameof (EncryptedProperties), "main").LogDebug(ILogger_Fields.SECURITY, "Loading encrypted properties from " + fileInfo.FullName);
      if (!File.Exists(fileInfo.FullName) && !Directory.Exists(fileInfo.FullName))
        throw new IOException("Properties file not found: " + fileInfo.FullName);
      Logger.GetLogger(nameof (EncryptedProperties), "main").LogDebug(ILogger_Fields.SECURITY, "Encrypted properties found in " + fileInfo.FullName);
      EncryptedProperties encryptedProperties = new EncryptedProperties();
      FileStream fileStream1 = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
      encryptedProperties.Load((Stream) fileStream1);
      StreamReader streamReader = new StreamReader(Console.OpenStandardInput());
      string key1;
      do
      {
        Console.Out.Write("Enter key: ");
        key1 = streamReader.ReadLine();
        Console.Out.Write("Enter value: ");
        string str = streamReader.ReadLine();
        if (key1 != null && key1.Length > 0 && str.Length > 0)
          encryptedProperties.SetProperty(key1, str);
      }
      while (key1 != null && key1.Length > 0);
      FileStream fileStream2 = new FileStream(fileInfo.FullName, FileMode.Create);
      encryptedProperties.Store((Stream) fileStream2, "Encrypted Properties File");
      fileStream2.Close();
      foreach (string key2 in (IEnumerable) encryptedProperties.KeySet())
      {
        string property = encryptedProperties.GetProperty(key2);
        Console.Out.WriteLine("   " + key2 + "=" + property);
      }
    }
  }
}
