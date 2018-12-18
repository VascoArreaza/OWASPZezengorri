// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.AccessReferenceMap
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using Owasp.Esapi.Errors;
using Owasp.Esapi.Interfaces;
using System.Collections;

namespace Owasp.Esapi
{
  public class AccessReferenceMap : IAccessReferenceMap
  {
    internal Hashtable itod = new Hashtable();
    internal Hashtable dtoi = new Hashtable();
    internal IRandomizer random;

    private void InitBlock()
    {
      this.random = Owasp.Esapi.Esapi.Randomizer();
    }

    public AccessReferenceMap()
    {
      this.InitBlock();
    }

    public AccessReferenceMap(IList directReferences)
    {
      this.InitBlock();
      this.Update(directReferences);
    }

    public IEnumerator Enumerator()
    {
      return new SortedList((IDictionary) this.dtoi).Keys.GetEnumerator();
    }

    public void AddDirectReference(string direct)
    {
      string randomString = this.random.GetRandomString(6, Encoder.CHAR_ALPHANUMERICS);
      this.itod[(object) randomString] = (object) direct;
      this.dtoi[(object) direct] = (object) randomString;
    }

    public void RemoveDirectReference(string direct)
    {
      string str = (string) this.dtoi[(object) direct];
      if (str == null)
        return;
      this.itod.Remove((object) str);
      this.dtoi.Remove((object) direct);
    }

    public void Update(IList directReferences)
    {
      Hashtable hashtable = (Hashtable) this.dtoi.Clone();
      this.dtoi.Clear();
      this.itod.Clear();
      foreach (object directReference in (IEnumerable) directReferences)
      {
        string randomString = (string) hashtable[directReference];
        if (randomString == null)
        {
          do
          {
            randomString = this.random.GetRandomString(6, Encoder.CHAR_ALPHANUMERICS);
          }
          while (new ArrayList(this.itod.Keys).Contains((object) randomString));
        }
        this.itod[(object) randomString] = directReference;
        this.dtoi[directReference] = (object) randomString;
      }
    }

    public string GetIndirectReference(object directReference)
    {
      return (string) this.dtoi[directReference];
    }

    public object GetDirectReference(string indirectReference)
    {
      IEnumerator enumerator = (IEnumerator) this.dtoi.GetEnumerator();
      while (enumerator.MoveNext())
      {
        DictionaryEntry current = (DictionaryEntry) enumerator.Current;
      }
      if (this.itod.ContainsKey((object) indirectReference))
        return this.itod[(object) indirectReference];
      throw new AccessControlException("Access denied", "Request for invalid indirect reference");
    }
  }
}
