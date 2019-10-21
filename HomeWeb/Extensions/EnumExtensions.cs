using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Home.Web.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum targetEnum)
        {
            var type = targetEnum.GetType();
            var fields = type.IsEnum ? type.GetFields().Where(r => r.FieldType.Name == type.Name).ToArray() : new FieldInfo[] { };
            var fieldInfo = fields.FirstOrDefault(r => targetEnum.Equals(r.GetValue(r)));
            var attribute =
                fieldInfo?.GetCustomAttributes(true)
                    .FirstOrDefault(r => r.GetType().Name == nameof(DisplayNameAttribute)) as DisplayNameAttribute;
            return attribute?.Name;
        }
        public static IEnumerable<KeyValuePair<int, string>> GetEnumOptions<T>() where T : Enum
        {
            var options = new List<KeyValuePair<int, string>>();
            var type = typeof(T);
            var fields = type.IsEnum ? type.GetFields().Where(r => r.FieldType.Name == type.Name).ToArray() : new FieldInfo[] { };
            foreach (var fieldInfo in fields)
            {
                var attribute =
                    fieldInfo?.GetCustomAttributes(true)
                        .FirstOrDefault(r => r.GetType().Name == nameof(DisplayNameAttribute)) as DisplayNameAttribute;
                if (attribute != null) options.Add(new KeyValuePair<int, string>((int)fieldInfo.GetValue(fieldInfo), attribute.Name));
            }
            return options;
        }
    }
}
