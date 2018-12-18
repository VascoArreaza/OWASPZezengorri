// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Encryptor
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using Owasp.Esapi.Errors;
using Owasp.Esapi.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Owasp.Esapi
{
  public class Encryptor : IEncryptor
  {
    private static readonly Logger logger = Logger.GetLogger("ESAPI", nameof (Encryptor));
    internal string encryptAlgorithm = "Rijndael";
    internal string signatureAlgorithm = "DSA";
    internal string hashAlgorithm = "SHA-512";
    internal string randomAlgorithm = "SHA1PRNG";
    internal string encoding = "UTF-8";
    private byte[] secretKey;
    private byte[] iv;
    internal CspParameters asymmetricKeyPair;

    public long TimeStamp
    {
      get
      {
        return DateTime.Now.Ticks;
      }
    }

    public Encryptor()
    {
      byte[] masterSalt = Owasp.Esapi.Esapi.SecurityConfiguration().MasterSalt;
      string masterPassword = Owasp.Esapi.Esapi.SecurityConfiguration().MasterPassword;
      this.encryptAlgorithm = Owasp.Esapi.Esapi.SecurityConfiguration().EncryptionAlgorithm;
      this.signatureAlgorithm = Owasp.Esapi.Esapi.SecurityConfiguration().DigitalSignatureAlgorithm;
      this.randomAlgorithm = Owasp.Esapi.Esapi.SecurityConfiguration().RandomAlgorithm;
      this.hashAlgorithm = Owasp.Esapi.Esapi.SecurityConfiguration().HashAlgorithm;
      try
      {
        SymmetricAlgorithm symmetricAlgorithm = SymmetricAlgorithm.Create(this.encryptAlgorithm);
        symmetricAlgorithm.GenerateIV();
        this.iv = symmetricAlgorithm.IV;
        symmetricAlgorithm.Padding = PaddingMode.PKCS7;
        this.secretKey = new PasswordDeriveBytes(masterPassword, masterSalt).CryptDeriveKey(this.encryptAlgorithm, "SHA1", symmetricAlgorithm.KeySize, this.iv);
        this.encoding = Owasp.Esapi.Esapi.SecurityConfiguration().CharacterEncoding;
        this.asymmetricKeyPair = new CspParameters(13);
        this.asymmetricKeyPair.KeyContainerName = "ESAPI";
        RandomNumberGenerator.Create(this.randomAlgorithm);
      }
      catch (Exception ex)
      {
        EncryptionException encryptionException = new EncryptionException("Encryption failure", "Error creating Encryptor", ex);
      }
    }

    public string Hash(string plaintext, string salt)
    {
      try
      {
        HashAlgorithm hashAlgorithm = HashAlgorithm.Create(this.hashAlgorithm);
        byte[] bytes = Encoding.UTF8.GetBytes(plaintext + salt);
        byte[] hash = hashAlgorithm.ComputeHash(bytes);
        for (int index = 0; index < 1024; ++index)
          hash = hashAlgorithm.ComputeHash(hash);
        return Convert.ToBase64String(hash);
      }
      catch (Exception ex)
      {
        throw new EncryptionException("Internal error", "Can't find hash algorithm " + this.hashAlgorithm, ex);
      }
    }

    public string Encrypt(string plaintext)
    {
      try
      {
        ICryptoTransform encryptor = SymmetricAlgorithm.Create(this.encryptAlgorithm).CreateEncryptor(this.secretKey, this.iv);
        MemoryStream memoryStream = new MemoryStream();
        CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, encryptor, CryptoStreamMode.Write);
        byte[] bytes = Encoding.GetEncoding(this.encoding).GetBytes(plaintext);
        cryptoStream.Write(bytes, 0, bytes.Length);
        cryptoStream.FlushFinalBlock();
        return Convert.ToBase64String(memoryStream.ToArray());
      }
      catch (Exception ex)
      {
        throw new EncryptionException("Encryption failure", "Decryption problem: " + ex.Message, ex);
      }
    }

    public string Decrypt(string ciphertext)
    {
      try
      {
        ICryptoTransform decryptor = SymmetricAlgorithm.Create(this.encryptAlgorithm).CreateDecryptor(this.secretKey, this.iv);
        CryptoStream cryptoStream = new CryptoStream((Stream) new MemoryStream(Convert.FromBase64String(ciphertext)), decryptor, CryptoStreamMode.Read);
        byte[] numArray = new byte[Convert.FromBase64String(ciphertext).Length];
        cryptoStream.Read(numArray, 0, numArray.Length);
        return Encoding.GetEncoding(this.encoding).GetString(numArray);
      }
      catch (Exception ex)
      {
        throw new EncryptionException("Decryption failed", "Decryption problem: " + ex.Message, ex);
      }
    }

    public string Sign(string data)
    {
      try
      {
        DSACryptoServiceProvider cryptoServiceProvider = new DSACryptoServiceProvider(this.asymmetricKeyPair);
        byte[] bytes = Encoding.GetEncoding(this.encoding).GetBytes(data);
        byte[] numArray = cryptoServiceProvider.SignData(bytes);
        cryptoServiceProvider.VerifyData(bytes, numArray);
        return Owasp.Esapi.Esapi.Encoder().EncodeForBase64(numArray, true);
      }
      catch (Exception ex)
      {
        throw new EncryptionException("Signature failure", "Can't find signature algorithm " + this.signatureAlgorithm, ex);
      }
    }

    public bool VerifySignature(string signature, string data)
    {
      try
      {
        DSACryptoServiceProvider cryptoServiceProvider = new DSACryptoServiceProvider(this.asymmetricKeyPair);
        Encoding encoding = Encoding.GetEncoding(this.encoding);
        byte[] rgbSignature = Owasp.Esapi.Esapi.Encoder().DecodeFromBase64(signature);
        byte[] bytes = encoding.GetBytes(data);
        return cryptoServiceProvider.VerifyData(bytes, rgbSignature);
      }
      catch (Exception ex)
      {
        EncryptionException encryptionException = new EncryptionException("Invalid signature", "Problem verifying signature: " + ex.Message, ex);
        return false;
      }
    }

    public string Seal(string data, long expiration)
    {
      try
      {
        return this.Encrypt(expiration.ToString() + ":" + data);
      }
      catch (EncryptionException ex)
      {
        throw new IntegrityException(ex.UserMessage, ex.LogMessage, (Exception) ex);
      }
    }

    public bool VerifySeal(string seal, string data)
    {
      string str;
      try
      {
        str = this.Decrypt(seal);
      }
      catch (EncryptionException ex)
      {
        EncryptionException encryptionException = new EncryptionException("Invalid seal", "Seal did not decrypt properly", (Exception) ex);
        return false;
      }
      int length = str.IndexOf(":");
      if (length == -1)
      {
        EncryptionException encryptionException = new EncryptionException("Invalid seal", "Seal did not contain properly formatted separator");
        return false;
      }
      if (DateTime.Now.Ticks > long.Parse(str.Substring(0, length)))
      {
        EncryptionException encryptionException = new EncryptionException("Invalid seal", "Seal expiration date has expired");
        return false;
      }
      if (str.Substring(length + 1).Equals(data))
        return true;
      EncryptionException encryptionException1 = new EncryptionException("Invalid seal", "Seal data does not match");
      return false;
    }
  }
}
