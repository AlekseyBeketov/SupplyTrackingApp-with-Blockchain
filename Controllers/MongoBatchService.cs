using Blockchain_Supply_Chain_Tracking_System.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blockchain_Supply_Chain_Tracking_System.Services
{
    public class MongoBatchService
    {
        private readonly IMongoCollection<Batch> _batchCollection;

        public MongoBatchService(IMongoDatabase database)
        {
            _batchCollection = database.GetCollection<Batch>("Batches");
        }

        public async Task SaveBatchAsync(Batch batch)
        {
            await _batchCollection.InsertOneAsync(batch);
        }

        public async Task<List<Batch>> GetAllBatchesAsync()
        {
            return await _batchCollection.Find(_ => true).ToListAsync();
        }
    }
}
