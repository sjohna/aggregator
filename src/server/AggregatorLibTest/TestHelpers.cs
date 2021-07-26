using LiteDB;
using NUnit.Framework;

namespace AggregatorLibTest
{
    public static class TestHelpers
    {
        public class Container<T>
        {
            [BsonId]
            public int Id = 1;

            public T? Value { get; protected set; }

            public Container() { }

            public Container(T value)
            {
                this.Value = value;
            }
        }

        public static T AssertNotNull<T>(T? value)
        {
            if (value == null)
            {
                Assert.IsNotNull(value);
                throw new System.Exception();   // shouldn't be reached, but will silence the compiler
            }
            else
            {
                return value;
            }
        }

        public static Container<T> InContainer<T>(this T value)
        {
            return new Container<T>(value);
        }
    }
}
