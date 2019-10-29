using System;
using MongoDB.Driver;

namespace AspNetCore.Identity.Mongo.Mongo
{
    public interface IMongoService
    {
        IMongoCollection<T> GetCollection<T>(Action<IMongoCollection<T>> action);
    }
}