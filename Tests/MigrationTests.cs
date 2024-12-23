using AspNetCore.Identity.Mongo.Migrations;
using AspNetCore.Identity.Mongo.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests;

[TestFixture]
public class MigrationTests
{
    private IDisposable _runner;
    private IMongoClient _client;
    private IMongoDatabase _db;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var runner = Mongo2Go.MongoDbRunner.Start();
        _client = new MongoClient(runner.ConnectionString);
        _db = _client.GetDatabase("migration-tests");
        _runner = runner;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _runner.Dispose();
    }

    [Test, Category("unit")]
    public void Apply_Schema4_AllMigrationsApplied()
    {
        // ARRANGE
        var history = _db.GetCollection<MigrationHistory>("migrations");
        var users = _db.GetCollection<MigrationMongoUser>("users");
        var roles = _db.GetCollection<MongoRole<ObjectId>>("roles");
        var initialVersion = 4;
        var existingHistory = new List<MigrationHistory>
            {
                new MigrationHistory
                {
                    Id = ObjectId.GenerateNewId(),
                    DatabaseVersion = 3,
                    InstalledOn = DateTime.UtcNow.AddDays(-2)
                },
                new MigrationHistory
                {
                    Id = ObjectId.GenerateNewId(),
                    DatabaseVersion = initialVersion,
                    InstalledOn = DateTime.UtcNow.AddDays(-1)
                }
            };
        history.InsertMany(existingHistory);


        // ACT
        Migrator.Apply<MigrationMongoUser, MongoRole<ObjectId>, ObjectId>(history, users, roles);

        // ASSERT
        var historyAfter = history
            .Find("{}")
            .SortBy(h => h.DatabaseVersion)
            .ToList();

        var expectedHistoryObjectsAfter = Migrator.CurrentVersion - initialVersion + existingHistory.Count;
        Assert.That(historyAfter.Count, Is.EqualTo(expectedHistoryObjectsAfter),
            () => "Expected all migrations to run");
        Assert.That(historyAfter.Last().DatabaseVersion, Is.EqualTo(Migrator.CurrentVersion));
    }
}