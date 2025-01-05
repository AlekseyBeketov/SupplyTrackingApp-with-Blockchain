using MongoDB.Driver;
using Blockchain_Supply_Chain_Tracking_System.Models;
using System.Linq.Expressions;
using MongoDB.Bson;

namespace Blockchain_Supply_Chain_Tracking_System.Services
{
    public class MongoUserGroupService
    {
        private readonly IMongoCollection<UserGroup> _userGroupCollection;

        public MongoUserGroupService(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("SupplyTrackingDb");
            _userGroupCollection = database.GetCollection<UserGroup>("UserGroup");
        }

        public async Task SaveUserGroupAsync(UserGroup userGroup)
        {
            await _userGroupCollection.InsertOneAsync(userGroup);
        }
        public async Task InsertIntoUserGroupAsync(string userGroupId, int transporterId)
        {
            try
            {
                var objectId = new ObjectId(userGroupId);
                var filter = Builders<UserGroup>.Filter.Eq("_id", objectId);
                var update = Builders<UserGroup>.Update.AddToSet(u => u.UserIds, transporterId);

                var result = await _userGroupCollection.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    throw new Exception($"No UserGroup found with ID: {userGroupId}");
                }
            }
            catch (FormatException)
            {
                throw new Exception($"Invalid format for UserGroup ID: {userGroupId}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to insert transporter into UserGroup: {ex.Message}");
            }
        }

        public async Task<List<UserGroup>> GetAllUsersAsync()
        {
            return await _userGroupCollection.Find(_ => true).ToListAsync();
        }
        public async Task<List<UserGroup>> GetUserGroupsByConditionAsync(Expression<Func<UserGroup, bool>> condition)
        {
            var filter = Builders<UserGroup>.Filter.Where(condition);
            return await _userGroupCollection.Find(filter).ToListAsync();
        }
    }
}
