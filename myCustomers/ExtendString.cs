using System.Drawing;

namespace myCustomers
{
    public static class ExtendString
    {
        public static string ToHtmlHexColor(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var parts = value.Split(',');
            if (parts.Length != 3)
                return null;

            return ColorTranslator.ToHtml(Color.FromArgb(0, int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2])));
        }
    }
}
