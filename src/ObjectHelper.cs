namespace System
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;

    static class ObjectHelper<T> where T : class, new()
    {
        private static readonly PropertyInfo[] _mergeProperties = (
            from pi in GetAllProperties(typeof(T))
            where pi.GetMethod != null && pi.GetMethod.IsPublic && pi.SetMethod != null && pi.SetMethod.IsPublic
            select pi).ToArray();

        private static readonly FieldInfo[] _mergeFields = (
            from fi in GetAllFields(typeof(T)) where fi.IsPublic select fi).ToArray();

        public static T Merge(T target, T change)
        {
            if (ReferenceEquals(target, change)) return target;

            foreach (var pi in _mergeProperties)
            {
                pi.SetMethod.Invoke(target, new[] { pi.GetMethod.Invoke(change, null) });
            }

            foreach (var pi in _mergeFields)
            {
                pi.SetValue(target, pi.GetValue(change));
            }

            return target;
        }

        public static T MergeExclude(T target, T change, string[] excludedProperties)
        {
            if (ReferenceEquals(target, change)) return target;

            foreach (var pi in _mergeProperties)
            {
                if (excludedProperties.Contains(pi.Name)) continue;
                pi.SetMethod.Invoke(target, new[] { pi.GetMethod.Invoke(change, null) });
            }

            foreach (var pi in _mergeFields)
            {
                if (excludedProperties.Contains(pi.Name)) continue;
                pi.SetValue(target, pi.GetValue(change));
            }

            return target;
        }

        public static T Clone(T target)
        {
            if (target == null) return null;
            return Merge(new T(), target);
        }

        public static IEnumerable<string> Delta(T target, T comparand)
        {
            if (ReferenceEquals(target, comparand)) yield break;

            foreach (var prop in _mergeProperties)
            {
                if (!Equals(prop.GetMethod.Invoke(target, null),
                            prop.GetMethod.Invoke(comparand, null)))
                {
                    yield return prop.Name;
                }
            }

            foreach (var field in _mergeFields)
            {
                if (!Equals(field.GetValue(target),
                            field.GetValue(target)))
                {
                    yield return field.Name;
                }
            }
        }

        private static IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            while (type != null)
            {
                var ti = type.GetTypeInfo();
                foreach (var prop in ti.DeclaredProperties)
                {
                    yield return prop;
                }
                type = ti.BaseType;
            }
        }

        private static IEnumerable<FieldInfo> GetAllFields(Type type)
        {
            while (type != null)
            {
                var ti = type.GetTypeInfo();
                foreach (var field in ti.DeclaredFields)
                {
                    yield return field;
                }
                type = ti.BaseType;
            }
        }
    }
}
