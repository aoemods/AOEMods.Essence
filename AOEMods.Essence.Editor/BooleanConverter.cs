using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace AOEMods.Essence.Editor;

public class BooleanConverter<TValue> : IValueConverter where TValue : notnull
{
    public BooleanConverter(TValue trueValue, TValue falseValue)
    {
        TrueValue = trueValue;
        FalseValue = falseValue;
    }

    public TValue TrueValue
    {
        get;
        set;
    }

    public TValue FalseValue
    {
        get;
        set;
    }

    public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? TrueValue : FalseValue;
    }

    public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is TValue val && EqualityComparer<TValue>.Default.Equals(val, TrueValue);
    }
}