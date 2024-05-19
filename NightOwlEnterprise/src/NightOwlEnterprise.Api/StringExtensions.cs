using System.Linq.Expressions;
using System.Text;

namespace NightOwlEnterprise.Api;

public static class StringExtensions
{
    public static string ReplaceTurkishCharacters(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        StringBuilder stringBuilder = new StringBuilder(input.Length);

        foreach (char c in input)
        {
            switch (c)
            {
                case 'ı':
                    stringBuilder.Append('i');
                    break;
                case 'İ':
                    stringBuilder.Append('I');
                    break;
                case 'ğ':
                    stringBuilder.Append('g');
                    break;
                case 'Ğ':
                    stringBuilder.Append('G');
                    break;
                case 'ş':
                    stringBuilder.Append('s');
                    break;
                case 'Ş':
                    stringBuilder.Append('S');
                    break;
                case 'ç':
                    stringBuilder.Append('c');
                    break;
                case 'Ç':
                    stringBuilder.Append('C');
                    break;
                case 'ü':
                    stringBuilder.Append('u');
                    break;
                case 'Ü':
                    stringBuilder.Append('U');
                    break;
                case 'ö':
                    stringBuilder.Append('o');
                    break;
                case 'Ö':
                    stringBuilder.Append('O');
                    break;
                case 'â':
                    stringBuilder.Append('a');
                    break;
                case 'Â':
                    stringBuilder.Append('A');
                    break;
                default:
                    stringBuilder.Append(c);
                    break;
            }
        }

        return stringBuilder.ToString();
    }
}