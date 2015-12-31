namespace System
{
    using System.Reflection;
    using System.Linq;
    using System.Collections.Generic;

    static class ObjectHelper<T> where T : new()
    {
        private static readonly PropertyInfo[] properties = (
            from pi in typeof(T).GetTypeInfo().DeclaredProperties
            where pi.GetMethod != null && pi.GetMethod.IsPublic && 
                  pi.SetMethod != null && pi.SetMethod.IsPublic &&
                  pi.GetIndexParameters().Length <= 0
            select pi).ToArray();

        private static readonly FieldInfo[] fields = (
            from fi in typeof(T).GetTypeInfo().DeclaredFields where fi.IsPublic select fi).ToArray();

        public static T Merge(T target, T change)
        {
            if (Equals(target, change)) return target;

            foreach (var prop in properties)
            {
                prop.SetMethod.Invoke(target, new[] { prop.GetMethod.Invoke(change, null) });
            }

            foreach (var field in fields)
            {
                field.SetValue(target, field.GetValue(change));
            }

            return target;
        }

        public static T MergeExclude(T target, T change, string[] excludedProperties)
        {
            if (Equals(target, change)) return target;

            foreach (var prop in properties)
            {
                if (excludedProperties.Contains(prop.Name)) continue;
                prop.SetMethod.Invoke(target, new[] { prop.GetMethod.Invoke(change, null) });
            }

            foreach (var field in fields)
            {
                if (excludedProperties.Contains(field.Name)) continue;
                field.SetValue(target, field.GetValue(change));
            }

            return target;
        }

        public static T Clone(T target)
        {
            return Merge(new T(), target);
        }

        public static IEnumerable<string> Delta(T target, T comparand)
        {
            if (Equals(target, comparand)) yield break;

            foreach (var prop in properties)
            {
                if (!Equals(prop.GetMethod.Invoke(target, null), 
                            prop.GetMethod.Invoke(comparand, null)))
                {
                    yield return prop.Name;
                }
            }

            foreach (var field in fields)
            {
                if (!Equals(field.GetValue(target),
                            field.GetValue(target)))
                {
                    yield return field.Name;
                }
            }

        }
    }
}
