using System;

namespace RevBridge.Functions
{
    public static class ExtensionMethods
    {
        public static T ParseEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
        }
    }
}
