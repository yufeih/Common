namespace Nine.Formatting
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Newtonsoft.Json;
    using ProtoBuf;
    using Xunit;

    public class FormatterSpec
    {
        private readonly MemoryStream _ms = new MemoryStream(8000);

        public static TheoryData<IFormatter> Formatters => _formatters.Value;

        private static readonly Lazy<TheoryData<IFormatter>> _formatters = new Lazy<TheoryData<IFormatter>>(() =>
        {
            return new TheoryData<IFormatter>
            {
                new ProtoFormatter(),
                new JsonFormatter(),
                new JilFormatter(),
                new BsonFormatter(),
            };
        });

        [Theory, MemberData(nameof(Formatters))]
        public void it_should_format_basic_types(IFormatter formatter)
        {
            var a = new BasicTypes();
            var b = PingPong(formatter, a, text =>
            {
                Console.WriteLine($"{formatter.GetType().Name}:\n{text}\n");
            });
            Assert.Equal(JsonConvert.SerializeObject(a), JsonConvert.SerializeObject(b));
        }

        [Theory, MemberData(nameof(Formatters))]
        public void it_should_not_format_private_members(IFormatter formatter)
        {
            var a = new BasicTypes();
            var b = PingPong(formatter, a,
                text => Assert.False(text.Contains("notSerialized")));
        }

        [Theory, MemberData(nameof(Formatters))]
        public void i_can_add_new_fields(IFormatter formatter)
        {
            var a = new BasicTypes();
            AddNewField b = PingPong<BasicTypes, AddNewField>(formatter, a);
            Assert.Equal(a.String, b.String);
        }

        [Theory, MemberData(nameof(Formatters))]
        public void i_can_remove_existing_fields(IFormatter formatter)
        {
            var a = new AddNewField();
            var b = PingPong<BasicTypes>(formatter, a);
            Assert.Equal(a.String, b.String);
        }

        [Theory, MemberData(nameof(Formatters))]
        public void it_should_fallback_to_default_if_an_enum_value_is_not_found(IFormatter formatter)
        {
            if (formatter is ProtoFormatter || formatter is BsonFormatter || formatter is JilFormatter) return;

            var b = PingPong<EnumClassA, EnumClassB>(formatter, new EnumClassA { Comparison = "OrdinalIgnoreCase" });
            Assert.Equal(StringComparison.OrdinalIgnoreCase, b.Comparison);

            b = PingPong<EnumClassA, EnumClassB>(formatter, new EnumClassA { Comparison = "OrdinalIgnoreCase111" });
            Assert.Equal((StringComparison)0, b.Comparison);
        }

        [Theory, MemberData(nameof(Formatters))]
        public void i_can_turn_fields_into_nullable(IFormatter formatter)
        {
            var a = new NotNullable();
            var b = PingPong<NotNullable, Nullable>(formatter, a);
            Assert.Equal(a.Value, b.Value);
        }

        [Theory, MemberData(nameof(Formatters))]
        public void formatter_speed(IFormatter formatter)
        {
            var sw = Stopwatch.StartNew();
            var a = new BasicTypes();
            var iterations = 10000;
            for (int i = 0; i < iterations; i++)
            {
                PingPong<BasicTypes, BasicTypes>(formatter, a);
            }
            Console.WriteLine("[perf]> " + formatter.GetType().Name + " \t" + sw.Elapsed.TotalMilliseconds / iterations + "ms");
        }

        // [Fact] Time handling is different
        public void jil_to_jsonnet()
        {
            var a = new BasicTypes();
            var b = new JsonFormatter().FromText<BasicTypes>(new JilFormatter().ToText(new BasicTypes()));

            Assert.Equal(JsonConvert.SerializeObject(a), JsonConvert.SerializeObject(b));
        }

        private T PingPong<T>(IFormatter formatter, T value, Action<string> action = null)
        {
            var sw = Stopwatch.StartNew();
            var bytes = formatter.ToBytes(value);
            var writeMs = sw.ElapsedMilliseconds;

            if (action != null)
            {
                var textFormatter = formatter as ITextFormatter;
                if (textFormatter != null)
                {
                    action(textFormatter.ToText(value));
                }
            }

            sw.Restart();
            var result = formatter.FromBytes<T>(bytes);
            Console.WriteLine($"{formatter.GetType().Name}\twrite: {writeMs}ms\tread: {sw.ElapsedMilliseconds}ms\t{_ms.Length}");
            return result;
        }

        private T2 PingPong<T1, T2>(IFormatter formatter, T1 value)
        {
            return formatter.FromBytes<T2>(formatter.ToBytes(value));
        }

        [ProtoContract]
        public class AddNewField : BasicTypes
        {
            [ProtoMember(51)]
            public int? IWontBreakYou = int.MinValue;
        }

        [ProtoContract]
        public class NotNullable
        {
            [ProtoMember(1)]
            public long Value = long.MinValue;
        }

        [ProtoContract]
        public class Nullable
        {
            [ProtoMember(1)]
            public long? Value;
        }

        public class EnumClassA
        {
            public string Comparison { get; set; }
        }

        public class EnumClassB
        {
            public StringComparison Comparison { get; set; }
        }
    }
}
