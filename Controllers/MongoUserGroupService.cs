using MongoDB.Driver;
using Blockchain_Supply_Chain_Tracking_System.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using MongoDB.Bson;

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
        public async Task InsertIntoUserGroupAsync(string userGroupId, int transporterId)
        {
            try
            {
                // Преобразование строки userGroupId в ObjectId
                var objectId = new ObjectId(userGroupId);

                // Строим фильтр для поиска документа по _id
                var filter = Builders<UserGroup>.Filter.Eq("_id", objectId);

                // Строим обновление для добавления transporterId в массив UserIds
                var update = Builders<UserGroup>.Update.AddToSet(u => u.UserIds, transporterId);

                // Выполняем обновление
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
            // Применяем фильтр для выбора записей, соответствующих условию
            var filter = Builders<UserGroup>.Filter.Where(condition);

            // Выполняем запрос и возвращаем полный список документов
            return await _userGroupCollection.Find(filter).ToListAsync();
        }

    }
}
