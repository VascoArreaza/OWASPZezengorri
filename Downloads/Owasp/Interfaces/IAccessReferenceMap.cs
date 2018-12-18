// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Interfaces.IAccessReferenceMap
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using System.Collections;

namespace Owasp.Esapi.Interfaces
{
  public interface IAccessReferenceMap
  {
    IEnumerator Enumerator();

    string GetIndirectReference(object directReference);

    object GetDirectReference(string indirectReference);
  }
}
