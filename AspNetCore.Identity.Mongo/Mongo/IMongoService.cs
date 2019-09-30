using System;
using MongoDB.Driver;

namespace Maddalena.Mongo
{
    public interface IMongoService
    {
        IMongoCollection<T> GetCollection<T>(Action<IMongoCollection<T>> action);
    }
}