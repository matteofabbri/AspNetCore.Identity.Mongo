using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace AspNetCore.Identity.Mongo.Model;

public class MigrationHistory : IComparable<MigrationHistory>, IComparable
{
    /// <summary>Mongo storing Id</summary>
    public ObjectId Id { get; set; }

    public int DatabaseVersion { get; set; }
    public DateTime InstalledOn { get; set; }

    public int CompareTo(MigrationHistory other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return DatabaseVersion.CompareTo(other.DatabaseVersion);
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