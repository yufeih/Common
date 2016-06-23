namespace Nine.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    static class MarkerInterface
    {
        public static Dictionary<Type, T> GroupByGenericTypeArgument<T>(this T[] values, Type typeDefinition, bool throwOnDuplicate = true)
        {
            var result = new Dictionary<Type, T>();
            if (values == null) return result;

            foreach (var value in values)
            {
                if (value == null) continue;

                var types =
                    from i in value.GetType().GetTypeInfo().ImplementedInterfaces
                    where i.GetTypeInfo().IsGenericType
                    let d = i.GetGenericTypeDefinition()
                    where d == typeDefinition
                    select i.GenericTypeArguments[0];

                foreach (var type in types)
                {
                    T existing;
                    if (result.TryGetValue(type, out existing))
                    {
                        var error = type.FullName + " is implemented by both " + existing.GetType().FullName + " and " + value.GetType().FullName;
                        if (throwOnDuplicate) throw new InvalidOperationException(error);
                        Debug.WriteLine(error);
                    }
                    result[type] = value;
                }
            }
            return result;
        }
    }
}
