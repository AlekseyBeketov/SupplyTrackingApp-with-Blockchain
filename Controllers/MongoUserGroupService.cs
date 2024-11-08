﻿using MongoDB.Driver;
using Blockchain_Supply_Chain_Tracking_System.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq.Expressions;

namespace Blockchain_Supply_Chain_Tracking_System.Services
{
    public class MongoUserGroupService
    {
        private readonly IMongoCollection<UserGroup> _userGroupCollection;

        public MongoUserGroupService(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("SupplyTrackingDb"); // Убедитесь, что имя БД корректное
            _userGroupCollection = database.GetCollection<UserGroup>("UserGroup");
        }

        public async Task SaveUserGroupAsync(UserGroup userGroup)
        {
            await _userGroupCollection.InsertOneAsync(userGroup);
        }
        public async Task<List<UserGroup>> GetAllUsersAsync()
        {
            return await _userGroupCollection.Find(_ => true).ToListAsync();
        }
        public async Task<List<string>> GetUserGroupIdsByConditionAsync(Expression<Func<UserGroup, bool>> condition)
        {
            var filter = Builders<UserGroup>.Filter.Where(condition);
            var projection = Builders<UserGroup>.Projection.Include(u => u.GroupId);
            var result = await _userGroupCollection.Find(filter).Project(projection).ToListAsync();

            return result.Select(u => u["_id"].AsObjectId.ToString()).Distinct().ToList();
        }
    }
}
