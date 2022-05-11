using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DeskTopTimer.Converter
{
    public class WidthHalfConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value==null)
                return Binding.DoNothing;
            var curValue = value as double?;
            var percent = double.Parse(parameter as string);

            return curValue==null? Binding.DoNothing: curValue.Value/ percent;
         
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    class MultiBindingConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Image image = values[0] as Image;
            System.Windows.Size size = image.RenderSize;
            double x = size.Width;
            double y = size.Height;
            return new System.Windows.Rect(0, 0, x, y);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class BoolToReverseVisi : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool bo)
                return bo?Visibility.Collapsed:Visibility.Visible;
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
    class BoolToVisi : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bo)
                return bo ? Visibility.Visible : Visibility.Collapsed;
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }


    public class WidthAndHeightToRectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values[0] == null || values[1] == null)
                    return Binding.DoNothing;
                double width = (double)values[0];
                double height = (double)values[1];
                return new Rect(0, 0, width, height);
            }
            catch(Exception ex)
            {
                return Binding.DoNothing;
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return new object[]{ Binding.DoNothing };
        }
    }


    public class WebSiteToVisiableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values[0] == null || values[0] == DependencyProperty.UnsetValue || values[1] == null || values[1] == DependencyProperty.UnsetValue)
                return Binding.DoNothing;
            var IsWebSiteVisiable = (bool)(values[0]);
            var IsObjectVisiable= (bool)(values[1]);
            return IsWebSiteVisiable?Visibility.Collapsed:IsObjectVisiable?Visibility.Visible:Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
           return new object[]{ Binding.DoNothing };
        }
    }

    public class WebSiteToReverseVisiableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            
            if (values[0] == null||values[0]== DependencyProperty.UnsetValue || values[1] == null || values[1] == DependencyProperty.UnsetValue)
                return Binding.DoNothing;
            var IsWebSiteVisiable = (bool)(values[0]);
            var IsObjectVisiable = (bool)(values[1]);
            return IsWebSiteVisiable ? Visibility.Collapsed : IsObjectVisiable ? Visibility.Collapsed : Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { Binding.DoNothing };
        }
    }


    public class FontNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value!=null&&value is FontFamily font)
            {
                return font.GetLocalizedName();
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }


    public class CtrlKeyToVisi : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var curModifyer = (ModifierKeys)value;
            return curModifyer.HasFlag(ModifierKeys.Control) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class ShiftKeyToVisi : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var curModifyer = (ModifierKeys)value;
            return curModifyer.HasFlag(ModifierKeys.Shift) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class AltKeyToVisi : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var curModifyer = (ModifierKeys)value;
            return curModifyer.HasFlag(ModifierKeys.Alt) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class WinKeyToVisi : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var curModifyer = (ModifierKeys)value;
            return curModifyer.HasFlag(ModifierKeys.Windows) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }


    public class KeyToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var curKey = (Key)value;
            if (curKey != Key.None)
            {
                if (curKey == Key.OemOpenBrackets)
                    return "[";
                else if (curKey == Key.OemCloseBrackets)
                    return "]";
                else if (curKey == Key.Add)
                    return "+";
                else if (curKey == Key.Subtract)
                    return "-";
                else if (curKey == Key.OemSemicolon)
                    return ";";
                else if (curKey == Key.OemQuotes)
                    return ":";
                else if (curKey == Key.OemQuestion)
                    return "?";
                else if (curKey == Key.Separator)
                    return "|";
                else if (curKey == Key.OemComma)
                    return ",";
                else if (curKey == Key.OemPeriod)
                    return ".";
                else
                    return Enum.GetName(typeof(Key), curKey);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        } }
    }
