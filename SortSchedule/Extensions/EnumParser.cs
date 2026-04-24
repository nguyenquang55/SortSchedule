using System.Text;

namespace SortSchedule.Extensions;

public static class EnumParser
{
    public static TEnum ParseStrictEnum<TEnum>(this string? value, string? paramName = null)
        where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        var normalizedValue = value.Normalize(NormalizationForm.FormC).Trim();
        if (normalizedValue.Length == 0)
        {
            throw new ArgumentException("Enum value cannot be empty.", paramName);
        }

        return Enum.Parse<TEnum>(normalizedValue, ignoreCase: true);
    }
}
