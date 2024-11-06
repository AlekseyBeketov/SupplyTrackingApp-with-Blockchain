using Blockchain_Supply_Chain_Tracking_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Blockchain_Supply_Chain_Tracking_System.Services;
using System.Security.Claims;
using System.Linq;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;
using System.Text;
using System.Security.Cryptography;


namespace Blockchain_Supply_Chain_Tracking_System.Controllers
{
    public class TrackingModel
    {
        public List<User> Users { get; set; }
        public List<Vendor> Vendors { get; set; }
        public List<Vendorproduct> VendorProducts { get; set; }
        public List<Blockchainblock> Blockchainblocks { get; set; }
    }

    public class CreateBlockchainRequest
    {
        public string BatchId { get; set; }
        public string UserGroupId { get; set; }
        public int UserId { get; set; }
        public string DetailsOrder { get; set; }
        public string Signature { get; set; }
    }

    public class TrackingController : Controller
    {
        private readonly SupplyTrackingContext _supplyTrackingContext;
        private readonly MongoBatchService _mongoBatchService;
        private readonly MongoUserGroupService _mongoUserGroupService; // Сервис для работы с коллекцией UserGroup
        private List<Blockchainblock> blockchain;
        private int difficulty;
        private IJsEngine jsEngine;
        public TrackingController(SupplyTrackingContext supplyTrackingContext, MongoBatchService mongoBatchService, MongoUserGroupService mongoUserGroupService)
        {
            _supplyTrackingContext = supplyTrackingContext;
            _mongoBatchService = mongoBatchService;
            _mongoUserGroupService = mongoUserGroupService;
        }

        [Authorize]
        public async Task<IActionResult> Index(int selectedVendor = 1)
        {
            if (selectedVendor == 0)
            {
                selectedVendor = 1;
                ViewBag.PageFirstLoad = true;
            }
            else
            {
                ViewBag.PageFirstLoad = false;
            }

            // Получаем всех пользователей
            List<User> users = _supplyTrackingContext.Users.ToList();
            List<Blockchainblock> blockchain = _supplyTrackingContext.Blockchainblocks.ToList();
            // Получаем всех вендоров из таблицы Vendors
            List<Vendor> vendors = _supplyTrackingContext.Vendors.ToList();
            List<Vendorproduct> vendorproduct = _supplyTrackingContext.Vendorproducts.ToList();

            Vendor selectedVendorDetails = _supplyTrackingContext.Vendors.FirstOrDefault(v => v.Vendorid == selectedVendor);
            List<Vendorproduct> selectetVendorProducts = _supplyTrackingContext.Vendorproducts.Where(p => p.Vendorid == selectedVendor).ToList();

            // Получаем все партии из MongoDB
            List<Batch> batches = await _mongoBatchService.GetAllBatchesAsync();
            ViewBag.Batches = batches;

            // Передаем данные в ViewBag для использования в представлении
            ViewBag.SelectedVendor = selectedVendor;
            ViewBag.SelectedVendorDetails = selectedVendorDetails;
            ViewBag.VendorProducts = selectetVendorProducts;

            var viewModel = new TrackingModel
            {
                Users = users,
                Vendors = vendors,
                VendorProducts = vendorproduct,
                Blockchainblocks = blockchain
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveBatch([FromBody] Batch batch)
        {
            if (batch != null && batch.Products.Any())
            {
                try
                {
                    // Получение ID пользователя, создающего партию
                    if (User.Identity.IsAuthenticated)
                    {
                        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                        if (userIdClaim != null)
                        {
                            batch.UserId = int.Parse(userIdClaim.Value);
                        }
                    }
                    // Сохранение партии в MongoDB
                    await _mongoBatchService.SaveBatchAsync(batch);

                    int vendorId = (int)_supplyTrackingContext.Vendorproducts.FirstOrDefault(v => v.Productid == batch.Products.FirstOrDefault().ProductId)?.Vendorid;

                    // Создание записи в UserGroup после успешного создания Batch
                    var userGroup = new UserGroup
                    {
                        UserIds = new List<int> { batch.UserId, vendorId } // Добавляем UserId пользователя, создавшего партию и VendorId
                    };

                    // Сохранение UserGroup в MongoDB
                    await _mongoUserGroupService.SaveUserGroupAsync(userGroup);

                    // Возвращаем идентификаторы партии и группы пользователей
                    return Json(new { success = true, message = "Batch and UserGroup saved to MongoDB", batchId = batch.BatchId, userGroupId = userGroup.GroupId, userId = batch.UserId });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error saving to MongoDB: " + ex.Message });
                }
            }

            return Json(new { success = false, message = "Error: No data to save" });
        }


        [HttpPost]
        public IActionResult CreateBlockhain([FromBody] CreateBlockchainRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is null");
            }

            Console.WriteLine($"Received BatchId: {request.BatchId}, UserGroupId: {request.UserGroupId}, UserId: {request.UserId}, DetailsOrder: {request.DetailsOrder}");

            Blockchainid blockchainid = new Blockchainid { Blockchainid1 = request.BatchId, Usergroupid = request.UserGroupId };
            _supplyTrackingContext.Blockchainids.Add(blockchainid);

            // Создание генезис-блока
            blockchain = new List<Blockchainblock>();
            difficulty = 4;
            CreateGenesisBlock(request.BatchId, request.UserId, request.DetailsOrder);

            foreach (var block in blockchain)
            {
                _supplyTrackingContext.Blockchainblocks.Add(block);
            }

            _supplyTrackingContext.SaveChanges();
            return RedirectToAction("Index", new { selectedVendor = 1 });
        }

        private void CreateGenesisBlock(string BatchId, int UserId, string DetailsOrder)
        {
            string blockchainId = BatchId;
            int vendorId = 0;
            int clientId = UserId;
            int transporterId = 0;
            int userId = UserId;
            string batchId = BatchId;
            string eventType = "Поиск транспортировщика";
            string location = _supplyTrackingContext.Clients.FirstOrDefault(c => c.Userid == userId)?.Address;
            string previousHash = "0";
            string carNumber = "0";
            string eventDetails = DetailsOrder; 
            string publicKey = "хз";
            Blockchainblock genesisBlock = CreateBlock(blockchainId, vendorId, clientId, transporterId, userId, batchId, eventType, location, previousHash, carNumber, eventDetails, publicKey); // Правильно передать параметры
            MineBlock(genesisBlock);
            blockchain.Add(genesisBlock);
        }

        private Blockchainblock CreateBlock(string blockchainId, int vendorId, int clientId, int transporterId, int userId, string batchId, string eventType, string location, string previousHash, string carNumber, string eventDetails, string publicKey)
        {
            Blockchainblock block = new Blockchainblock
            {
                Blockchainid = blockchainId,
                Vendorid = vendorId,
                Clientid = clientId,
                Transporterid = transporterId,
                Userid = userId,
                Batchid = batchId,
                Eventtype = eventType,
                Location = location,
                Hash = "",
                Previoushash = previousHash,
                Carnumber = carNumber,
                Eventdetails = eventDetails,
                Timestampblock = DateTime.Now,
                Nonce = 0,
                Publickey = publicKey
            };
            Console.WriteLine($"Создан новый блок с данными: {blockchainId}");
            return block;
        }

        private string CalculateHash(Blockchainblock block)
        {
            string dataToHash = $"{block.Blockchainid}{block.Vendorid}{block.Clientid}{block.Transporterid}{block.Userid}{block.Batchid}{block.Eventtype}{block.Location}{block.Hash}{block.Previoushash}{block.Carnumber}{block.Hash}{block.Hash}";
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void MineBlock(Blockchainblock block)
        {
            string target = new string('0', difficulty);
            Console.WriteLine($"Начало майнинга блока с данными: {block.Blockchainid}");
            try
            {
                while (!block.Hash.StartsWith(target))
                {
                    block.Nonce++;
                    block.Hash = CalculateHash(block);
                    if (block.Nonce % 100000 == 0)
                    {
                        Console.WriteLine($"Майнинг... nonce: {block.Nonce}");
                    }
                }
                Console.WriteLine($"Блок успешно замайнен: {block.Hash}");
            }
            catch (ThreadInterruptedException)
            {
                Console.WriteLine("Майнинг прерван пользователем.");
            }
        }
    }
}
