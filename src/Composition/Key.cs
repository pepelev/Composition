using System;

namespace Composition
{
    internal readonly struct Key : IEquatable<Key>
    {
        private readonly string id;
        private readonly Type type;

        public Key(string id, Type type)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException(
                    "Must be non-empty",
                    nameof(id)
                );
            }

            this.id = id;
            this.type = type;
        }

        public bool Equals(Key other) => id == other.id && type == other.type;
        public override bool Equals(object obj) => obj is Key other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (id.GetHashCode() * 397) ^ type.GetHashCode();
            }
        }
    }
}