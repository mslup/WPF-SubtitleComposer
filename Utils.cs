using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace lab8
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value is not TimeSpan timeSpan)
                return null;

            if (timeSpan.TotalSeconds >= 1)
                return timeSpan.ToString(@"hh\:mm\:ss\.FFF").Trim('0', ':').TrimEnd('.');

            return $"0{(timeSpan.TotalMilliseconds > 0 ? "." : "")}{timeSpan:FFF}";
        }

        public object ConvertBack(object value, Type targetType,
             object parameter, CultureInfo culture)
        {

            if (value is not string timeSpanStr)
                return TimeSpan.Zero;

            if (string.IsNullOrEmpty(timeSpanStr))
                return TimeSpan.Zero;

            int count = timeSpanStr.Count(c => c == ':');
            TimeSpan ret;
            switch (count)
            {
            case 0:
                return TimeSpan.TryParse($"00:00:{timeSpanStr}", out ret) ?
                    ret : TimeSpan.Zero;
            case 1:
                return TimeSpan.TryParse($"00:{timeSpanStr}", out ret) ?
                    ret : TimeSpan.Zero;
            default:
                return TimeSpan.TryParse(timeSpanStr, out ret) ?
                    ret : TimeSpan.Zero;
            }
        }
    }

    public class TextToHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
           object parameter, CultureInfo culture)
        {
            if (value is not string str)
                return "";

            return $"Text: {str.Length} characters";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TranslationToHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
           object parameter, CultureInfo culture)
        {
            if (value is not string str)
                return "";

            return $"Translation: {str.Length} characters";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TimeSpanValidation : ValidationRule
    {
        public Regex r = new Regex(@"^\d+(?::\d{0,2}){0,2}(?:\.\d{0,3})?$",
            RegexOptions.Compiled);

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (r.IsMatch((string)value))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, "Invalid result");
        }
    }
}
