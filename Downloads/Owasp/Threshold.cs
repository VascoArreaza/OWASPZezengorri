// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Threshold
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using System.Collections;

namespace Owasp.Esapi
{
  public class Threshold
  {
    public string Name = (string) null;
    public int Count = 0;
    public long Interval = 0;
    public IList Actions = (IList) null;

    public Threshold(string name, int count, long interval, IList actions)
    {
      this.Name = name;
      this.Count = count;
      this.Interval = interval;
      this.Actions = actions;
    }

    public override string ToString()
    {
      return "Threshold: " + this.Name + " - " + (object) this.Count + " in " + (object) this.Interval + " seconds results in " + this.Actions.ToString();
    }
  }
}
