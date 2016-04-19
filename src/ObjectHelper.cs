namespace System
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;

    static class ObjectHelper
    {
        private static Func<object, object> _memberwiseClone;

        static ObjectHelper()
        {
            var clone = typeof(object).GetTypeInfo().GetDeclaredMethod("MemberwiseClone");
            _memberwiseClone = (Func<object, object>)clone.CreateDelegate(typeof(Func<object, object>));
        }

        public static T MemberwiseClone<T>(this T target) where T : class
        {
            if (target == null) return null;
            return (T)_memberwiseClone(target);
        }

        public static T With<T>(this T target, T change, string property) where T : class, new()
        {
            var result = _memberwiseClone(target);

            foreach (var pi in Accessor<T>.Properties)
            {
                if (pi.Member.Name == property)
                {
                    pi.Copy(change, result);
                    return (T)result;
                }
            }

            foreach (var fi in Accessor<T>.Fields)
            {
                if (fi.Name == property)
                {
                    fi.SetValue(result, fi.GetValue(change));
                    return (T)result;
                }
            }

            return (T)result;
        }

        public static T With<T>(this T target, T change, string property, params string[] properties) where T : class, new()
        {
            var result = _memberwiseClone(target);

            var found = false;

            foreach (var pi in Accessor<T>.Properties)
            {
                if (pi.Member.Name == property)
                {
                    pi.Copy(change, result);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                foreach (var fi in Accessor<T>.Fields)
                {
                    if (fi.Name == property)
                    {
                        fi.SetValue(result, fi.GetValue(change));
                        break;
                    }
                }
            }

            foreach (var p in properties)
            {
                found = false;

                foreach (var pi in Accessor<T>.Properties)
                {
                    if (pi.Member.Name == p)
                    {
                        pi.Copy(change, result);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    foreach (var fi in Accessor<T>.Fields)
                    {
                        if (fi.Name == p)
                        {
                            fi.SetValue(result, fi.GetValue(change));
                            break;
                        }
                    }
                }
            }

            return (T)result;
        }

        public static T Merge<T>(this T target, T change) where T : class, new()
        {
            if (ReferenceEquals(target, change)) return target;

            foreach (var pi in Accessor<T>.Properties)
            {
                pi.Copy(change, target);
            }

            foreach (var fi in Accessor<T>.Fields)
            {
                fi.SetValue(target, fi.GetValue(change));
            }

            return target;
        }

        public static T Merge<T>(this T target, T change, Func<MemberInfo, bool> predicate) where T : class, new()
        {
            if (ReferenceEquals(target, change)) return target;

            foreach (var pi in Accessor<T>.Properties)
            {
                if (predicate(pi.Member))
                {
                    pi.Copy(change, target);
                }
            }

            foreach (var pi in Accessor<T>.Fields)
            {
                if (predicate(pi))
                {
                    pi.SetValue(target, pi.GetValue(change));
                }
            }

            return target;
        }

        public static IEnumerable<string> Delta<T>(this T target, T comparand) where T : class, new()
        {
            if (ReferenceEquals(target, comparand)) yield break;

            foreach (var pi in Accessor<T>.Properties)
            {
                if (pi.Compare(target, comparand))
                {
                    yield return pi.Member.Name;
                }
            }

            foreach (var fi in Accessor<T>.Fields)
            {
                if (!Equals(fi.GetValue(target),
                            fi.GetValue(target)))
                {
                    yield return fi.Name;
                }
            }
        }

        interface IPropertyAccessor
        {
            MemberInfo Member { get; }
            object GetValue(object source);
            void SetValue(object source, object value);
            void Copy(object source, object target);
            bool Compare(object source, object target);
        }

        class PropertyAccessor<T, TValue> : IPropertyAccessor
        {
            private readonly Func<T, TValue> _getter;
            private readonly Action<T, TValue> _setter;
            private readonly PropertyInfo _pi;

            public MemberInfo Member => _pi;

            public PropertyAccessor(PropertyInfo pi)
            {
                _pi = pi;
                _getter = (Func<T, TValue>)pi.GetMethod.CreateDelegate(typeof(Func<T, TValue>));
                _setter = (Action<T, TValue>)pi.SetMethod.CreateDelegate(typeof(Action<T, TValue>));
            }

            public object GetValue(object source) => _getter((T)source);
            public void SetValue(object source, object value) => _setter((T)source, (TValue)value);
            public void Copy(object source, object target) => _setter((T)target, _getter((T)source));
            public bool Compare(object source, object target) => Equals(_getter((T)source), _getter((T)target));
        }

        static class Accessor<T> where T : class, new()
        {
            public static readonly IPropertyAccessor[] Properties = (
                from pi in GetAllProperties(typeof(T))
                where pi.GetMethod != null && pi.GetMethod.IsPublic && pi.SetMethod != null && pi.SetMethod.IsPublic
                let accessorType = typeof(PropertyAccessor<,>).MakeGenericType(typeof(T), pi.PropertyType)
                select (IPropertyAccessor)Activator.CreateInstance(accessorType, pi)).ToArray();

            public static readonly FieldInfo[] Fields = (
                from fi in GetAllFields(typeof(T)) where fi.IsPublic select fi).ToArray();

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
}
