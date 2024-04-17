using System;
using System.Reflection;

namespace Msmp.Utility
{
    internal static class ReflectionUtility
    {
        public static T GetPrivateField<T>(this Type type, string fieldName, object @object)
        {
            return (T)type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(@object);
        }

        public static void SetPrivateField(this Type type, string fieldName, object value, object @object)
        {
            type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(@object, value);
        }
    }
}
