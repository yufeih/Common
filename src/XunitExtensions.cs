namespace Xunit
{
    using System.Collections.Generic;
    using System.Linq;

    static class XunitExtensions
    {
        public static TheoryData<T> ToTheoryData<T>(this IEnumerable<T> enumerable)
        {
            var data = new TheoryData<T>();
            foreach (var item in enumerable)
            {
                data.Add(item);
            }
            return data;
        }

        public static IEnumerable<T> ToEnumerable<T>(this TheoryData<T> theoryData)
        {
            return theoryData.Select(d => (T)d[0]);
        }
    }
}
