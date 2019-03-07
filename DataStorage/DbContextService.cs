﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;

namespace DataStorage
{
    public class MongoDbStorageService : IMongoDbStorage
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _db;
        public MongoDbStorageService()
        {
            /*var connection = new MongoUrlBuilder("mongodb://192.168.0.104:27017");*/
            //_client = new MongoClient("mongodb://localhost:27017");
            _client = new MongoClient("mongodb://192.168.0.104:27017");
            _db = _client.GetDatabase("home");
        }

        public bool IsConnected => _client.Cluster.Description.State == ClusterState.Connected;

        public async Task AddAsync<T>(string collection, T item)
        {
            if (IsConnected)
            {
                var list = _db.GetCollection<T>(collection);
                await list.InsertOneAsync(item);
            }
        }
        public async Task AddManyAsync<T>(string collection, IEnumerable<T> items)
        {
            if (IsConnected)
            {
                var list = _db.GetCollection<T>(collection);
                await list.InsertManyAsync(items);
            }
        }
        public async Task UpdateByIdAsync<T, TV>(string collection, Expression<Func<T, TV>> getId, T item)
        {
            if (IsConnected)
            {
                var id = getId.Compile();
                var filter = Builders<T>.Filter.Eq(getId, id(item));
                await _db.GetCollection<T>(collection).FindOneAndReplaceAsync(filter, item);
            }
        }
        public async Task<IEnumerable<T>> GetItemsAsync<T>(string collection)
        {
            if (IsConnected)
            {
                var list = await _db.GetCollection<T>(collection).Find(Builders<T>.Filter.Empty).ToListAsync();
                return list;
            }
            else
            {
                return await Task.FromResult(new List<T>());
            }
        }
        public async Task<IEnumerable<T>> FindAsync<T, TV>(string collection, Expression<Func<T, TV>> getField, TV fieldVal)
        {
            if (IsConnected)
            {

                var filter = Builders<T>.Filter.Eq(getField, fieldVal);
                var item = _db.GetCollection<T>(collection).Find(filter);
                var items = await item.ToListAsync();
                return items;
            }
            else
            {
                return await Task.FromResult(new List<T>());
            }
        }
    }
}