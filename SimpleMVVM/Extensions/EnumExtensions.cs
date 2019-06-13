using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SimpleMVVM.Extensions
{
    public static class EnumExtensions
    {
        public static string ToFriendlyString<T>(this T value) where T : struct, IConvertible // eneums
        {
            if (!typeof(T).IsEnum)
            {
                throw new Exception("Type given T must be an Enum");
            }

            FieldInfo fi = value.GetType().GetField(value.ToString());

            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            return attributes.Any() ? attributes[0].Description : fi.Name;
        }
    }
}
