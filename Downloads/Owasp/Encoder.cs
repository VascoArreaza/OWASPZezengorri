// Decompiled with JetBrains decompiler
// Type: Owasp.Esapi.Encoder
// Assembly: Owasp.Esapi, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6AEE56C8-668B-47C7-A220-9A96BB995F18
// Assembly location: C:\Users\u2\Downloads\Owasp.Esapi.dll

using Owasp.Esapi.Errors;
using Owasp.Esapi.Interfaces;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;

namespace Owasp.Esapi
{
  public class Encoder : IEncoder
  {
    public static readonly char[] CHAR_UPPERS = new char[36]
    {
      'A',
      'B',
      'C',
      'D',
      'E',
      'F',
      'G',
      'H',
      'I',
      'J',
      'K',
      'L',
      'M',
      'N',
      'O',
      'P',
      'Q',
      'R',
      'S',
      'T',
      'U',
      'V',
      'W',
      'X',
      'Y',
      'Z',
      '1',
      '2',
      '3',
      '4',
      '5',
      '6',
      '7',
      '8',
      '9',
      '0'
    };
    public static readonly char[] CHAR_LOWERS = new char[26]
    {
      'a',
      'b',
      'c',
      'd',
      'e',
      'f',
      'g',
      'h',
      'i',
      'j',
      'k',
      'l',
      'm',
      'n',
      'o',
      'p',
      'q',
      'r',
      's',
      't',
      'u',
      'v',
      'w',
      'x',
      'y',
      'z'
    };
    public static readonly char[] CHAR_DIGITS = new char[10]
    {
      '0',
      '1',
      '2',
      '3',
      '4',
      '5',
      '6',
      '7',
      '8',
      '9'
    };
    public static readonly char[] CHAR_PASSWORD = new char[63]
    {
      'a',
      'b',
      'c',
      'd',
      'e',
      'f',
      'g',
      'h',
      'j',
      'k',
      'l',
      'm',
      'n',
      'p',
      'q',
      'r',
      's',
      't',
      'u',
      'v',
      'w',
      'x',
      'y',
      'z',
      'A',
      'B',
      'C',
      'D',
      'E',
      'F',
      'G',
      'H',
      'J',
      'K',
      'L',
      'M',
      'N',
      'P',
      'Q',
      'R',
      'S',
      'T',
      'U',
      'V',
      'W',
      'X',
      'Y',
      'Z',
      '2',
      '3',
      '4',
      '5',
      '6',
      '7',
      '8',
      '9',
      '.',
      '!',
      '@',
      '$',
      '*',
      '=',
      '?'
    };
    public static readonly char[] CHAR_SPECIALS = new char[13]
    {
      '.',
      '-',
      '_',
      '!',
      '@',
      '$',
      '^',
      '*',
      '=',
      '~',
      '|',
      '+',
      '?'
    };
    private static readonly char[] IMMUNE_HTML = new char[5]
    {
      ',',
      '.',
      '-',
      '_',
      ' '
    };
    private static readonly char[] IMMUNE_HTMLATTR = new char[4]
    {
      ',',
      '.',
      '-',
      '_'
    };
    private static readonly char[] IMMUNE_JAVASCRIPT = new char[5]
    {
      ',',
      '.',
      '-',
      '_',
      ' '
    };
    private static readonly char[] IMMUNE_VBSCRIPT = new char[5]
    {
      ',',
      '.',
      '-',
      '_',
      ' '
    };
    private static readonly char[] IMMUNE_XML = new char[5]
    {
      ',',
      '.',
      '-',
      '_',
      ' '
    };
    private static readonly char[] IMMUNE_XMLATTR = new char[4]
    {
      ',',
      '.',
      '-',
      '_'
    };
    private static readonly char[] IMMUNE_XPATH = new char[5]
    {
      ',',
      '.',
      '-',
      '_',
      ' '
    };
    private static readonly Logger logger = Logger.GetLogger("ESAPI", nameof (Encoder));
    internal static readonly char[] CHAR_LETTERS = Randomizer.Union(Encoder.CHAR_LOWERS, Encoder.CHAR_UPPERS);
    public static readonly char[] CHAR_ALPHANUMERICS = Randomizer.Union(Encoder.CHAR_LETTERS, Encoder.CHAR_DIGITS);
    public const int NO_ENCODING = 0;
    public const int URL_ENCODING = 1;
    public const int PERCENT_ENCODING = 2;
    public const int ENTITY_ENCODING = 3;
    private static Hashtable characterToEntityMap;
    private static Hashtable entityToCharacterMap;

    static Encoder()
    {
      Encoder.InitializeMaps();
    }

    public string Canonicalize(string input)
    {
      StringBuilder stringBuilder = new StringBuilder();
      Encoder.EncodedStringReader encodedStringReader = new Encoder.EncodedStringReader(input);
      while (encodedStringReader.HasNext())
      {
        Encoder.EncodedCharacter nextCharacter = encodedStringReader.NextCharacter;
        if (nextCharacter != null)
          stringBuilder.Append(nextCharacter.Unencoded);
      }
      return stringBuilder.ToString();
    }

    public virtual string Normalize(string input)
    {
      string str = input.Normalize(NormalizationForm.FormD);
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < str.Length; ++index)
      {
        char ch = str[index];
        if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark && ch <= '\x007F')
          stringBuilder.Append(ch);
      }
      return stringBuilder.ToString();
    }

    private bool IsContained(char[] array, char element)
    {
      for (int index = 0; index < array.Length; ++index)
      {
        if ((int) element == (int) array[index])
          return true;
      }
      return false;
    }

    private string EntityEncode(string input, char[] baseChars, char[] immune)
    {
      StringBuilder stringBuilder = new StringBuilder();
      Encoder.EncodedStringReader encodedStringReader = new Encoder.EncodedStringReader(input);
      while (encodedStringReader.HasNext())
      {
        Encoder.EncodedCharacter nextCharacter = encodedStringReader.NextCharacter;
        if (nextCharacter != null)
        {
          if (this.IsContained(baseChars, nextCharacter.Unencoded) || this.IsContained(immune, nextCharacter.Unencoded))
            stringBuilder.Append(nextCharacter.Unencoded);
          else
            stringBuilder.Append(nextCharacter.GetEncoded(3));
        }
      }
      return stringBuilder.ToString();
    }

    public string EncodeForHtml(string input)
    {
      return this.EntityEncode(input, Encoder.CHAR_ALPHANUMERICS, Encoder.IMMUNE_HTML).Replace("\r", "<BR>").Replace("\n", "<BR>");
    }

    public string EncodeForHtmlAttribute(string input)
    {
      return this.EntityEncode(input, Encoder.CHAR_ALPHANUMERICS, Encoder.IMMUNE_HTMLATTR);
    }

    public string EncodeForJavascript(string input)
    {
      return this.EntityEncode(input, Encoder.CHAR_ALPHANUMERICS, Encoder.IMMUNE_JAVASCRIPT);
    }

    public string EncodeForVbScript(string input)
    {
      return this.EntityEncode(input, Encoder.CHAR_ALPHANUMERICS, Encoder.IMMUNE_VBSCRIPT);
    }

    public string EncodeForSql(string input)
    {
      return this.Canonicalize(input).Replace("'", "''");
    }

    public string EncodeForLdap(string input)
    {
      string str = this.Canonicalize(input);
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < str.Length; ++index)
      {
        char ch = str[index];
        switch (ch)
        {
          case char.MinValue:
            stringBuilder.Append("\\00");
            break;
          case '(':
            stringBuilder.Append("\\28");
            break;
          case ')':
            stringBuilder.Append("\\29");
            break;
          case '*':
            stringBuilder.Append("\\2a");
            break;
          case '\\':
            stringBuilder.Append("\\5c");
            break;
          default:
            stringBuilder.Append(ch);
            break;
        }
      }
      return stringBuilder.ToString();
    }

    public string EncodeForDn(string input)
    {
      string str = this.Canonicalize(input);
      StringBuilder stringBuilder = new StringBuilder();
      if (str.Length > 0 && (str[0] == ' ' || str[0] == '#'))
        stringBuilder.Append('\\');
      for (int index = 0; index < str.Length; ++index)
      {
        char ch = str[index];
        switch (ch)
        {
          case '"':
            stringBuilder.Append("\\\"");
            break;
          case '+':
            stringBuilder.Append("\\+");
            break;
          case ',':
            stringBuilder.Append("\\,");
            break;
          case ';':
            stringBuilder.Append("\\;");
            break;
          case '<':
            stringBuilder.Append("\\<");
            break;
          case '>':
            stringBuilder.Append("\\>");
            break;
          case '\\':
            stringBuilder.Append("\\\\");
            break;
          default:
            stringBuilder.Append(ch);
            break;
        }
      }
      if (str.Length > 1 && str[input.Length - 1] == ' ')
        stringBuilder.Insert(stringBuilder.Length - 1, '\\');
      return stringBuilder.ToString();
    }

    public string EncodeForXPath(string input)
    {
      return this.EntityEncode(input, Encoder.CHAR_ALPHANUMERICS, Encoder.IMMUNE_XPATH);
    }

    public string EncodeForXml(string input)
    {
      return this.EntityEncode(input, Encoder.CHAR_ALPHANUMERICS, Encoder.IMMUNE_XML);
    }

    public string EncodeForXmlAttribute(string input)
    {
      return this.EntityEncode(input, Encoder.CHAR_ALPHANUMERICS, Encoder.IMMUNE_XMLATTR);
    }

    public string EncodeForUrl(string input)
    {
      string str = this.Canonicalize(input);
      try
      {
        return HttpUtility.UrlEncode(str);
      }
      catch (IOException ex)
      {
        throw new EncodingException("Encoding failure", "Encoding not supported", (Exception) ex);
      }
      catch (Exception ex)
      {
        throw new EncodingException("Encoding failure", "Problem URL decoding input", ex);
      }
    }

    public string DecodeFromUrl(string input)
    {
      string str = this.Canonicalize(input);
      try
      {
        return HttpUtility.UrlDecode(str);
      }
      catch (IOException ex)
      {
        throw new EncodingException("Decoding failed", "Encoding not supported", (Exception) ex);
      }
      catch (Exception ex)
      {
        throw new EncodingException("Decoding failed", "Problem URL decoding input", ex);
      }
    }

    public string EncodeForBase64(byte[] input, bool wrap)
    {
      string str = Convert.ToBase64String(input);
      if (!wrap)
        str = str.Replace("\r", "").Replace("\n", "");
      return str;
    }

    public byte[] DecodeFromBase64(string input)
    {
      return Convert.FromBase64String(input);
    }

    private static void InitializeMaps()
    {
      string[] strArray = new string[252]
      {
        "quot",
        "amp",
        "lt",
        "gt",
        "nbsp",
        "iexcl",
        "cent",
        "pound",
        "curren",
        "yen",
        "brvbar",
        "sect",
        "uml",
        "copy",
        "ordf",
        "laquo",
        "not",
        "shy",
        "reg",
        "macr",
        "deg",
        "plusmn",
        "sup2",
        "sup3",
        "acute",
        "micro",
        "para",
        "middot",
        "cedil",
        "sup1",
        "ordm",
        "raquo",
        "frac14",
        "frac12",
        "frac34",
        "iquest",
        "Agrave",
        "Aacute",
        "Acirc",
        "Atilde",
        "Auml",
        "Aring",
        "AElig",
        "Ccedil",
        "Egrave",
        "Eacute",
        "Ecirc",
        "Euml",
        "Igrave",
        "Iacute",
        "Icirc",
        "Iuml",
        "ETH",
        "Ntilde",
        "Ograve",
        "Oacute",
        "Ocirc",
        "Otilde",
        "Ouml",
        "times",
        "Oslash",
        "Ugrave",
        "Uacute",
        "Ucirc",
        "Uuml",
        "Yacute",
        "THORN",
        "szlig",
        "agrave",
        "aacute",
        "acirc",
        "atilde",
        "auml",
        "aring",
        "aelig",
        "ccedil",
        "egrave",
        "eacute",
        "ecirc",
        "euml",
        "igrave",
        "iacute",
        "icirc",
        "iuml",
        "eth",
        "ntilde",
        "ograve",
        "oacute",
        "ocirc",
        "otilde",
        "ouml",
        "divide",
        "oslash",
        "ugrave",
        "uacute",
        "ucirc",
        "uuml",
        "yacute",
        "thorn",
        "yuml",
        "OElig",
        "oelig",
        "Scaron",
        "scaron",
        "Yuml",
        "fnof",
        "circ",
        "tilde",
        "Alpha",
        "Beta",
        "Gamma",
        "Delta",
        "Epsilon",
        "Zeta",
        "Eta",
        "Theta",
        "Iota",
        "Kappa",
        "Lambda",
        "Mu",
        "Nu",
        "Xi",
        "Omicron",
        "Pi",
        "Rho",
        "Sigma",
        "Tau",
        "Upsilon",
        "Phi",
        "Chi",
        "Psi",
        "Omega",
        "alpha",
        "beta",
        "gamma",
        "delta",
        "epsilon",
        "zeta",
        "eta",
        "theta",
        "iota",
        "kappa",
        "lambda",
        "mu",
        "nu",
        "xi",
        "omicron",
        "pi",
        "rho",
        "sigmaf",
        "sigma",
        "tau",
        "upsilon",
        "phi",
        "chi",
        "psi",
        "omega",
        "thetasym",
        "upsih",
        "piv",
        "ensp",
        "emsp",
        "thinsp",
        "zwnj",
        "zwj",
        "lrm",
        "rlm",
        "ndash",
        "mdash",
        "lsquo",
        "rsquo",
        "sbquo",
        "ldquo",
        "rdquo",
        "bdquo",
        "dagger",
        "Dagger",
        "bull",
        "hellip",
        "permil",
        "prime",
        "Prime",
        "lsaquo",
        "rsaquo",
        "oline",
        "frasl",
        "euro",
        "image",
        "weierp",
        "real",
        "trade",
        "alefsym",
        "larr",
        "uarr",
        "rarr",
        "darr",
        "harr",
        "crarr",
        "lArr",
        "uArr",
        "rArr",
        "dArr",
        "hArr",
        "forall",
        "part",
        "exist",
        "empty",
        "nabla",
        "isin",
        "notin",
        "ni",
        "prod",
        "sum",
        "minus",
        "lowast",
        "radic",
        "prop",
        "infin",
        "ang",
        "and",
        "or",
        "cap",
        "cup",
        "int",
        "there4",
        "sim",
        "cong",
        "asymp",
        "ne",
        "equiv",
        "le",
        "ge",
        "sub",
        "sup",
        "nsub",
        "sube",
        "supe",
        "oplus",
        "otimes",
        "perp",
        "sdot",
        "lceil",
        "rceil",
        "lfloor",
        "rfloor",
        "lang",
        "rang",
        "loz",
        "spades",
        "clubs",
        "hearts",
        "diams"
      };
      char[] chArray = new char[252]
      {
        '"',
        '&',
        '<',
        '>',
        ' ',
        '¡',
        '¢',
        '£',
        '¤',
        '¥',
        '¦',
        '§',
        '¨',
        '©',
        'ª',
        '«',
        '¬',
        '\x00AD',
        '®',
        '¯',
        '°',
        '±',
        '\x00B2',
        '\x00B3',
        '´',
        'µ',
        '¶',
        '·',
        '¸',
        '\x00B9',
        'º',
        '»',
        '\x00BC',
        '\x00BD',
        '\x00BE',
        '¿',
        'À',
        'Á',
        'Â',
        'Ã',
        'Ä',
        'Å',
        'Æ',
        'Ç',
        'È',
        'É',
        'Ê',
        'Ë',
        'Ì',
        'Í',
        'Î',
        'Ï',
        'Ð',
        'Ñ',
        'Ò',
        'Ó',
        'Ô',
        'Õ',
        'Ö',
        '×',
        'Ø',
        'Ù',
        'Ú',
        'Û',
        'Ü',
        'Ý',
        'Þ',
        'ß',
        'à',
        'á',
        'â',
        'ã',
        'ä',
        'å',
        'æ',
        'ç',
        'è',
        'é',
        'ê',
        'ë',
        'ì',
        'í',
        'î',
        'ï',
        'ð',
        'ñ',
        'ò',
        'ó',
        'ô',
        'õ',
        'ö',
        '÷',
        'ø',
        'ù',
        'ú',
        'û',
        'ü',
        'ý',
        'þ',
        'ÿ',
        'Œ',
        'œ',
        'Š',
        'š',
        'Ÿ',
        'ƒ',
        'ˆ',
        '˜',
        'Α',
        'Β',
        'Γ',
        'Δ',
        'Ε',
        'Ζ',
        'Η',
        'Θ',
        'Ι',
        'Κ',
        'Λ',
        'Μ',
        'Ν',
        'Ξ',
        'Ο',
        'Π',
        'Ρ',
        'Σ',
        'Τ',
        'Υ',
        'Φ',
        'Χ',
        'Ψ',
        'Ω',
        'α',
        'β',
        'γ',
        'δ',
        'ε',
        'ζ',
        'η',
        'θ',
        'ι',
        'κ',
        'λ',
        'μ',
        'ν',
        'ξ',
        'ο',
        'π',
        'ρ',
        'ς',
        'σ',
        'τ',
        'υ',
        'φ',
        'χ',
        'ψ',
        'ω',
        'ϑ',
        'ϒ',
        'ϖ',
        ' ',
        ' ',
        ' ',
        '\x200C',
        '\x200D',
        '\x200E',
        '\x200F',
        '–',
        '—',
        '‘',
        '’',
        '‚',
        '“',
        '”',
        '„',
        '†',
        '‡',
        '•',
        '…',
        '‰',
        '′',
        '″',
        '‹',
        '›',
        '‾',
        '⁄',
        '€',
        'ℑ',
        '℘',
        'ℜ',
        '™',
        'ℵ',
        '←',
        '↑',
        '→',
        '↓',
        '↔',
        '↵',
        '⇐',
        '⇑',
        '⇒',
        '⇓',
        '⇔',
        '∀',
        '∂',
        '∃',
        '∅',
        '∇',
        '∈',
        '∉',
        '∋',
        '∏',
        '∑',
        '−',
        '∗',
        '√',
        '∝',
        '∞',
        '∠',
        '∧',
        '∨',
        '∩',
        '∪',
        '∫',
        '∴',
        '∼',
        '≅',
        '≈',
        '≠',
        '≡',
        '≤',
        '≥',
        '⊂',
        '⊃',
        '⊄',
        '⊆',
        '⊇',
        '⊕',
        '⊗',
        '⊥',
        '⋅',
        '⌈',
        '⌉',
        '⌊',
        '⌋',
        '〈',
        '〉',
        '◊',
        '♠',
        '♣',
        '♥',
        '♦'
      };
      Encoder.characterToEntityMap = new Hashtable(strArray.Length);
      Encoder.entityToCharacterMap = new Hashtable(chArray.Length);
      for (int index = 0; index < strArray.Length; ++index)
      {
        string str = strArray[index];
        char ch = chArray[index];
        Encoder.entityToCharacterMap[(object) str] = (object) ch;
        Encoder.characterToEntityMap[(object) ch] = (object) str;
      }
    }

    [STAThread]
    public static void Main(string[] args)
    {
      Encoder encoder = new Encoder();
    }

    private class EncodedStringReader
    {
      internal string input = (string) null;
      internal int nextCharacter = 0;
      internal int testCharacter = 0;

      public Encoder.EncodedCharacter NextCharacter
      {
        get
        {
          this.testCharacter = this.nextCharacter;
          Encoder.EncodedCharacter encodedCharacter1 = this.PeekNextCharacter(this.input[this.nextCharacter]);
          this.nextCharacter = this.testCharacter;
          if (encodedCharacter1 == null)
            return (Encoder.EncodedCharacter) null;
          if (encodedCharacter1.IsEncoded())
          {
            --this.testCharacter;
            Encoder.EncodedCharacter encodedCharacter2 = this.PeekNextCharacter(encodedCharacter1.Unencoded);
            if (encodedCharacter2 != null && encodedCharacter2.IsEncoded())
              throw new IntrusionException("Validation error", "Input contains double encoded characters.");
          }
          return encodedCharacter1;
        }
      }

      public EncodedStringReader(string input)
      {
        if (input == null)
          this.input = "";
        else
          this.input = input;
      }

      public virtual bool HasNext()
      {
        return this.nextCharacter < this.input.Length;
      }

      private Encoder.EncodedCharacter PeekNextCharacter(char currentCharacter)
      {
        if (this.testCharacter == this.input.Length - 1)
        {
          ++this.testCharacter;
          return new Encoder.EncodedCharacter(currentCharacter);
        }
        switch (currentCharacter)
        {
          case '%':
            Encoder.EncodedCharacter percent = this.ParsePercent(this.input, this.testCharacter);
            if (percent != null)
              return percent;
            break;
          case '&':
            return this.ParseEntity(this.input, this.testCharacter);
        }
        ++this.testCharacter;
        return new Encoder.EncodedCharacter(currentCharacter);
      }

      public virtual Encoder.EncodedCharacter ParsePercent(string s, int startIndex)
      {
        string str = s.Substring(startIndex + 1, startIndex + 3 - (startIndex + 1));
        try
        {
          int int32 = Convert.ToInt32(str, 16);
          this.testCharacter += 3;
          return new Encoder.EncodedCharacter("%" + str, (char) int32, 2);
        }
        catch (FormatException ex)
        {
          return (Encoder.EncodedCharacter) null;
        }
      }

      public virtual Encoder.EncodedCharacter ParseEntity(string s, int startIndex)
      {
        int num1 = this.input.IndexOf(";", startIndex + 1);
        if (num1 != -1 && num1 - startIndex <= 8)
        {
          string lower = this.input.Substring(startIndex + 1, num1 - (startIndex + 1)).ToLower();
          if (Encoder.entityToCharacterMap[(object) lower] != null)
          {
            char entityToCharacter = (char) Encoder.entityToCharacterMap[(object) lower];
            this.testCharacter += lower.Length + 2;
            return new Encoder.EncodedCharacter("&" + lower + ";", entityToCharacter, 3);
          }
          if (lower[0] == '#')
          {
            this.testCharacter += lower.Length + 2;
            try
            {
              int num2 = int.Parse(lower.Substring(1));
              return new Encoder.EncodedCharacter("&#" + (object) (char) num2 + ";", (char) num2, 3);
            }
            catch (FormatException ex)
            {
              Encoder.logger.LogWarning(ILogger_Fields.SECURITY, "Invalid numeric entity encoding &" + lower + ";");
            }
          }
        }
        ++this.testCharacter;
        return new Encoder.EncodedCharacter("&", '&', 0);
      }
    }

    public class EncodedCharacter
    {
      internal string raw = "";
      internal char character = char.MinValue;
      internal int originalEncoding;

      public char Unencoded
      {
        get
        {
          return this.character;
        }
      }

      public EncodedCharacter(char character)
      {
        this.raw = "" + (object) character;
        this.character = character;
      }

      public virtual bool IsEncoded()
      {
        return this.raw.Length != 1;
      }

      public EncodedCharacter(string raw, char character, int originalEncoding)
      {
        this.raw = raw;
        this.character = character;
        this.originalEncoding = originalEncoding;
      }

      public virtual string GetEncoded(int encoding)
      {
        switch (encoding)
        {
          case 0:
            return "" + (object) this.character;
          case 1:
            if (char.IsWhiteSpace(this.character))
              return "+";
            if (char.IsLetterOrDigit(this.character))
              return "" + (object) this.character;
            return "%" + (object) (int) this.character;
          case 2:
            return "%" + (object) (int) this.character;
          case 3:
            string characterToEntity = (string) Encoder.characterToEntityMap[(object) this.character];
            if (characterToEntity != null)
              return "&" + characterToEntity + ";";
            return "&#" + (object) (int) this.character + ";";
          default:
            return (string) null;
        }
      }
    }
  }
}
