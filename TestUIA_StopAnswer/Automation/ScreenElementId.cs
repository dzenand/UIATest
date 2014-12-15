using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TestUIA.Automation
{
    public class ScreenElementId : IEquatable<ScreenElementId>, IComparable<ScreenElementId>, IEnumerable<int>
    {
        private readonly int[] _id;

        /// <summary>
        /// For performance reasons.
        /// </summary>
        private readonly string _stringId;

        public ScreenElementId(IntPtr id)
            : this(new[] { id.ToInt32() })
        {
        }

        public ScreenElementId(int[] id)
        {
            _id = id;

            // format id string the same way Inspect does
            _stringId = String.Join(".", _id.Select(i => i.ToString("X")));

            //var builder = new StringBuilder();
            //Array.ForEach(_id, x => builder.Append(x));
            //_stringId = builder.ToString();
        }

        public bool Equals(ScreenElementId other)
        {
            if (ReferenceEquals(null, other))
                return false;

            return _id.SequenceEqual(other._id);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ScreenElementId);
        }

        public override int GetHashCode()
        {
            return _id.Aggregate(17, (current, part) => current * 31 + part.GetHashCode());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator ScreenElementId(IntPtr handle)
        {
            return new ScreenElementId(handle);
        }
        public static bool operator ==(ScreenElementId left, ScreenElementId right)
        {
            if (ReferenceEquals(null, left))
                return ReferenceEquals(null, right);

            return left.Equals(right);
        }

        public static bool operator !=(ScreenElementId left, ScreenElementId right)
        {
            if (ReferenceEquals(null, left))
                return !ReferenceEquals(null, right);

            return !left.Equals(right);
        }

        public static bool operator >(ScreenElementId left, ScreenElementId right)
        {
            if (ReferenceEquals(null, left))
                return false;

            return left.CompareTo(right) > 0;
        }

        public static bool operator <(ScreenElementId left, ScreenElementId right)
        {
            if (ReferenceEquals(null, left))
                return false;

            return left.CompareTo(right) < 0;
        }

        public int CompareTo(ScreenElementId other)
        {
            if (other == null)
                return 1;

            if (Equals(other))
                return 0;

            return string.Compare(_stringId, other._stringId, StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerator<int> GetEnumerator()
        {
            return _id.AsEnumerable().GetEnumerator();
        }

        public override string ToString()
        {
            return _stringId;
        }

        public bool IsValid()
        {
            return (_id != null) && _id.Any();
        }
    }
}
