namespace System
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using System.Text;
    using Xunit;

    public class IdentifierHashTest
    {
        private static readonly Lazy<string[]> _identifiers = new Lazy<string[]>(() =>
        {
            return (
                from asm in Assembly.GetExecutingAssembly().LoadReferencedAssemblies(recursive: true)
                from type in asm.DefinedTypes
                from name in GetIdentifiers(type)
                where !name.Contains('_') && !name.Contains('@') &&
                      !name.Contains('`') && !name.Contains('.') &&
                      !name.Contains('<') && !name.Contains('>') &&
                      name.Length != 40 // GUID
                select name.ToLowerInvariant()).Distinct().OrderBy(_ => _).ToArray();
        });

        private static IEnumerable<string> GetIdentifiers(TypeInfo type)
        {
            yield return type.Name;
            foreach (var pi in type.DeclaredProperties) yield return pi.Name;
            foreach (var fi in type.DeclaredFields) yield return fi.Name;
            foreach (var e in type.DeclaredEvents) yield return e.Name;
            foreach (var m in type.DeclaredMethods)
            {
                yield return m.Name;
                foreach (var p in m.GetParameters()) yield return p.Name;
            }
        }

        [Fact]
        public void identifier_hash_collision_test()
        {
            var seed = 777u;
            var ids = _identifiers.Value;

            Console.WriteLine($"Total Identifier Count: {ids.Length}");
            Console.WriteLine($"Avg. Identifier Length: {ids.Average(n => n.Length)}");
            Console.WriteLine($"Max. Identifier Length: {ids.Max(n => n.Length)}");

            Test("Murmur3 32bit", ids, id => Murmur3.Hash32(Encoding.ASCII.GetBytes(id), seed));
            Test("Murmur3 16bit", ids, id => To16Bit(Murmur3.Hash32(Encoding.ASCII.GetBytes(id), seed)));

            Test("Identifier 32bit", ids, id => IdentifierHash.Hash(id, seed));
            Test("Identifier 16bit", ids, id => To16Bit(IdentifierHash.Hash(id, seed)));
        }

        private ushort To16Bit(uint value) => (ushort)((value & 0x0000FFFF) ^ (value >> 16));

        private void Test<T>(string name, string[] ids, Func<string, T> hash)
        {
            var collision = 0;
            var hashes = new HashSet<T>();

            foreach (var id in ids)
            {
                if (!hashes.Add(hash(id)))
                {
                    collision++;
                }
            }

            Console.WriteLine($"[{name}] \t{collision} ({collision * 100.0f / ids.Length}%) collisions.");
        }
    }
}
