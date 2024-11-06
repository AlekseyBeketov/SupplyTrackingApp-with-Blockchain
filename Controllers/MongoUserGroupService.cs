using MongoDB.Driver;
using Blockchain_Supply_Chain_Tracking_System.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

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
    }
}
