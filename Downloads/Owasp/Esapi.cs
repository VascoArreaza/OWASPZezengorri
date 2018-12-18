// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Esapi
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using Owasp.Esapi.Interfaces;

namespace Owasp.Esapi
{
  public class Esapi
  {
    private static IAccessController accessController = (IAccessController) null;
    private static IAuthenticator authenticator = (IAuthenticator) null;
    private static IEncoder encoder = (IEncoder) null;
    private static IEncryptor encryptor = (IEncryptor) null;
    private static IExecutor executor = (IExecutor) null;
    private static IHttpUtilities httpUtilities = (IHttpUtilities) null;
    private static IIntrusionDetector intrusionDetector = (IIntrusionDetector) null;
    private static IRandomizer randomizer = (IRandomizer) null;
    private static ISecurityConfiguration securityConfiguration = (ISecurityConfiguration) null;
    private static IValidator validator = (IValidator) null;

    private Esapi()
    {
    }

    public static IAccessController AccessController()
    {
      if (Owasp.Esapi.Esapi.accessController == null)
        Owasp.Esapi.Esapi.accessController = (IAccessController) new AccessController();
      return Owasp.Esapi.Esapi.accessController;
    }

    public static IAuthenticator Authenticator()
    {
      if (Owasp.Esapi.Esapi.authenticator == null)
        Owasp.Esapi.Esapi.authenticator = (IAuthenticator) new Authenticator();
      return Owasp.Esapi.Esapi.authenticator;
    }

    public static IEncoder Encoder()
    {
      if (Owasp.Esapi.Esapi.encoder == null)
        Owasp.Esapi.Esapi.encoder = (IEncoder) new Encoder();
      return Owasp.Esapi.Esapi.encoder;
    }

    public static IEncryptor Encryptor()
    {
      if (Owasp.Esapi.Esapi.encryptor == null)
        Owasp.Esapi.Esapi.encryptor = (IEncryptor) new Encryptor();
      return Owasp.Esapi.Esapi.encryptor;
    }

    public static IExecutor Executor()
    {
      if (Owasp.Esapi.Esapi.executor == null)
        Owasp.Esapi.Esapi.executor = (IExecutor) new Executor();
      return Owasp.Esapi.Esapi.executor;
    }

    public static IHttpUtilities HttpUtilities()
    {
      if (Owasp.Esapi.Esapi.httpUtilities == null)
        Owasp.Esapi.Esapi.httpUtilities = (IHttpUtilities) new HttpUtilities();
      return Owasp.Esapi.Esapi.httpUtilities;
    }

    public static IIntrusionDetector IntrusionDetector()
    {
      if (Owasp.Esapi.Esapi.intrusionDetector == null)
        Owasp.Esapi.Esapi.intrusionDetector = (IIntrusionDetector) new IntrusionDetector();
      return Owasp.Esapi.Esapi.intrusionDetector;
    }

    public static IRandomizer Randomizer()
    {
      if (Owasp.Esapi.Esapi.randomizer == null)
        Owasp.Esapi.Esapi.randomizer = (IRandomizer) new Randomizer();
      return Owasp.Esapi.Esapi.randomizer;
    }

    public static ISecurityConfiguration SecurityConfiguration()
    {
      if (Owasp.Esapi.Esapi.securityConfiguration == null)
        Owasp.Esapi.Esapi.securityConfiguration = (ISecurityConfiguration) new SecurityConfiguration();
      return Owasp.Esapi.Esapi.securityConfiguration;
    }

    public static IValidator Validator()
    {
      if (Owasp.Esapi.Esapi.validator == null)
        Owasp.Esapi.Esapi.validator = (IValidator) new Validator();
      return Owasp.Esapi.Esapi.validator;
    }
  }
}
