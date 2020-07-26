using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MongoDB.Bson;

namespace AspNetCore.Identity.Mongo.Model
{
    public class MigrationHistory : IComparable<MigrationHistory>, IComparable
    {
        /// <summary>Mongo storing Id</summary>
        public ObjectId Id { get; set; }

        /// <summary>Gets the value of the major component of the version number for the current <see cref="T:System.Version" /> object.</summary>
        /// <returns>The major version number.</returns>
        public int Major { get; }

        /// <summary>Gets the value of the minor component of the version number for the current <see cref="T:System.Version" /> object.</summary>
        /// <returns>The minor version number.</returns>
        public int Minor { get; }

        /// <summary>Gets the value of the revision component of the version number for the current <see cref="T:System.Version" /> object.</summary>
        /// <returns>The revision number, or -1 if the revision number is undefined.</returns>
        public int Revision { get; }

        /// <summary>Gets the value of the build component of the version number for the current <see cref="T:System.Version" /> object.</summary>
        /// <returns>The build number, or -1 if the build number is undefined.</returns>
        public int Build { get; }

        public int CompareTo(MigrationHistory other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var majorComparison = Major.CompareTo(other.Major);
            if (majorComparison != 0) return majorComparison;

            var minorComparison = Minor.CompareTo(other.Minor);
            if (minorComparison != 0) return minorComparison;

            var buildComparison = Build.CompareTo(other.Build);
            if (buildComparison != 0) return buildComparison;

            return Revision.CompareTo(other.Revision);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            return obj is MigrationHistory other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(MigrationHistory)}");
        }

        public static bool operator <(MigrationHistory left, MigrationHistory right)
        {
            return Comparer<MigrationHistory>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(MigrationHistory left, MigrationHistory right)
        {
            return Comparer<MigrationHistory>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(MigrationHistory left, MigrationHistory right)
        {
            return Comparer<MigrationHistory>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(MigrationHistory left, MigrationHistory right)
        {
            return Comparer<MigrationHistory>.Default.Compare(left, right) >= 0;
        }
    }
}
