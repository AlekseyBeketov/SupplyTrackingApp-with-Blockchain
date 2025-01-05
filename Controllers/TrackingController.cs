using Blockchain_Supply_Chain_Tracking_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Blockchain_Supply_Chain_Tracking_System.Services;
using System.Security.Claims;
using JavaScriptEngineSwitcher.Core;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Data;

namespace Blockchain_Supply_Chain_Tracking_System.Controllers
{
    public class TrackingController : Controller
    {
        private readonly SupplyTrackingContext _supplyTrackingContext;
        private readonly MongoBatchService _mongoBatchService;
        private readonly MongoUserGroupService _mongoUserGroupService;
        private List<Blockchainblock> blockchain;
        private int difficulty = 4;
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

            List<User> users = _supplyTrackingContext.Users.ToList();
            List<Vendor> vendors = _supplyTrackingContext.Vendors.ToList();
            List<Vendorproduct> vendorproduct = _supplyTrackingContext.Vendorproducts.ToList();
            List<Transporter> transporters = _supplyTrackingContext.Transporters.ToList();
            List<Client> clients = _supplyTrackingContext.Clients.ToList();
            Vendor selectedVendorDetails = _supplyTrackingContext.Vendors.FirstOrDefault(v => v.Vendorid == selectedVendor);
            List<Vendorproduct> selectetVendorProducts = _supplyTrackingContext.Vendorproducts.Where(p => p.Vendorid == selectedVendor).ToList();

            List<Batch> batches = await _mongoBatchService.GetAllBatchesAsync();
            ViewBag.Batches = batches;

            ViewBag.SelectedVendor = selectedVendor;
            ViewBag.SelectedVendorDetails = selectedVendorDetails;
            ViewBag.VendorProducts = selectetVendorProducts;

            int UserId = 0;
            string UserType = "";
            if (User.Identity!.IsAuthenticated)
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                var userTypeClaim = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Role);
                if (userIdClaim != null)
                {
                    UserId = int.Parse(userIdClaim.Value);
                    UserType = (userTypeClaim!.Value).ToString();
                }
            }

            List<UserGroup> userGroupIds = await _mongoUserGroupService.GetUserGroupsByConditionAsync(u => u.UserIds.Contains(UserId));
            List<Blockchainid> blockchainids = new List<Blockchainid>();
            List<Blockchainblock> blockchain = new List<Blockchainblock>();
            foreach (var groupId in userGroupIds)
            {
                var matchingBlockchainids = _supplyTrackingContext.Blockchainids
                    .Where(b => b.Usergroupid == groupId.GroupId)
                    .ToList();

                blockchainids.AddRange(matchingBlockchainids);
            }
            foreach (var blockchainid in blockchainids)
            {
                blockchain.AddRange(_supplyTrackingContext.Blockchainblocks.Where(b => b.Blockchainid == blockchainid.Blockchainid1).ToList());
            }

            if (UserType == "admin")
            {
                blockchain = _supplyTrackingContext.Blockchainblocks.ToList();
            }
            blockchainids.Reverse();
            var viewModel = new TrackingModel
            {
                Users = users,
                Vendors = vendors,
                VendorProducts = vendorproduct,
                Blockchainblocks = blockchain,
                Blockchainids = blockchainids,
                UserGroups = userGroupIds,
                Transporters = transporters,
                Clients = clients,
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
                    if (User.Identity!.IsAuthenticated)
                    {
                        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                        if (userIdClaim != null)
                        {
                            batch.UserId = int.Parse(userIdClaim.Value);
                        }
                    }


                    int vendorId = _supplyTrackingContext.Vendorproducts.FirstOrDefault(v => v.Productid == batch.Products.FirstOrDefault().ProductId)?.Vendorid ?? 0;
                    int userVendorId = _supplyTrackingContext.Vendors.FirstOrDefault(v => v.Vendorid == vendorId)?.Userid ?? 0;

                    var userGroup = new UserGroup
                    {
                        UserIds = new List<int> { batch.UserId, userVendorId }
                    };

                    await _mongoBatchService.SaveBatchAsync(batch);
                    await _mongoUserGroupService.SaveUserGroupAsync(userGroup);

                    return Json(new { success = true, message = "Batch and UserGroup saved to MongoDB", batchId = batch.BatchId, userGroupId = userGroup.GroupId, userId = batch.UserId });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error saving to MongoDB: " + ex.Message });
                }
            }

            return Json(new { success = false, message = "Error: No data to save" });
        }

		public async Task<bool> VerifyBlockSignature(Blockchainblock block)
		{
			try
			{
                var keyJson = JObject.Parse(block.Publickey); 
                var n = keyJson?["n"].ToString() ?? "";
                var e = keyJson?["e"].ToString() ?? "";

                n = n.Replace('-', '+').Replace('_', '/');
                n = AddBase64Padding(n);  

                e = e.Replace('-', '+').Replace('_', '/');
                e = AddBase64Padding(e);

                var modulus = Convert.FromBase64String(n);
                var exponent = Convert.FromBase64String(e);

                RSA rsa = RSA.Create();
                rsa.ImportParameters(new RSAParameters
                {
                    Modulus = modulus,
                    Exponent = exponent
                });


                byte[] signature = Convert.FromBase64String(block?.Signature ?? "");

                var batches = await _mongoBatchService.GetAllBatchesAsync();

                var record = block;

                if (record != null)
                {
					record.Signature = record.Publickey = "";
                    record.Blockid = 0;
				}

				string dataToVerify = JsonConvert.SerializeObject(record);

                dataToVerify = Regex.Replace(dataToVerify, @"\\", "");
                Console.WriteLine(dataToVerify);

                byte[] dataBytes = Encoding.UTF8.GetBytes(dataToVerify);

                return rsa.VerifyData(dataBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке подписи: {ex.Message}");
                return false;
			}
		}

        private string AddBase64Padding(string base64)
        {
            int padding = base64.Length % 4;
            if (padding != 0)
            {
                base64 = base64.PadRight(base64.Length + 4 - padding, '=');
            }
            return base64;
        }

        [HttpPost]
		public async Task<IActionResult> VerifyBlockSignature([FromBody] VerifyBlockSignatureRequest request)
		{
			var block = _supplyTrackingContext.Blockchainblocks
				.FirstOrDefault(b => b.Blockchainid == request.blockchainId && b.Blockid == request.blockNumber);

			if (block == null)
			{
				return Json(new { success = false, message = "Блок не найден" });
			}

			bool isVerified = await VerifyBlockSignature(block);

			if (isVerified)
			{
				return Json(new { success = true });
			}
			else
			{
				return Json(new { success = false, message = "Неверная подпись" });
			}
		}

		[HttpPost]
        public async Task<IActionResult> AssignTransporter(string blockchainId, int selectedTransporter, string details)
        {
            int userId = GetCurrentUserId();
            var userGroupId = _supplyTrackingContext.Blockchainids
                .FirstOrDefault(b => b.Blockchainid1 == blockchainId)?.Usergroupid;

            var transporterUserId = _supplyTrackingContext.Transporters.FirstOrDefault(t => t.Transporterid == selectedTransporter)?.Userid;

            if (string.IsNullOrEmpty(userGroupId))
            {
                return Json(new { success = false, message = "UserGroupId not found for the given BlockchainId" });
            }

            await _mongoUserGroupService.InsertIntoUserGroupAsync(userGroupId, transporterUserId ?? 0);
            var firstBlock = _supplyTrackingContext.Blockchainblocks.FirstOrDefault(b => b.Blockchainid == blockchainId);

            Blockchainblock newBlock = CreateBlock(blockchainId, firstBlock?.Vendorid ?? 0, firstBlock?.Clientid ?? 0,
                selectedTransporter, userId, firstBlock?.Batchid ?? "", "Ожидание транспортировки", firstBlock?.Location ?? "",
                firstBlock?.Hash ?? "", "0", details);
            _supplyTrackingContext.Blockchainblocks.Add(newBlock);
            await _supplyTrackingContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TransporterRecords(string blockchainId, string orderStatus, string carNumber, string location, string details)
        {
            int userId = GetCurrentUserId();
            var oldBlock = _supplyTrackingContext.Blockchainblocks
                .Where(b => b.Blockchainid == blockchainId)
                .OrderBy(b => b.Blockid)
                .LastOrDefault();

            Blockchainblock newBlock = CreateBlock(blockchainId, oldBlock?.Vendorid ?? 0, oldBlock?.Clientid ?? 0,
                oldBlock?.Transporterid ?? 0, userId, oldBlock?.Batchid, orderStatus, location ?? oldBlock?.Location ?? "",
                oldBlock?.Hash ?? "", carNumber, details);
            _supplyTrackingContext.Blockchainblocks.Add(newBlock);
            await _supplyTrackingContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ClientFinalStage(string blockchainId, string orderType, string details)
        {
            int userId = GetCurrentUserId();
            var oldBlock = _supplyTrackingContext.Blockchainblocks
                .Where(b => b.Blockchainid == blockchainId)
                .OrderBy(b => b.Blockid)
                .LastOrDefault();

            if (orderType == "Получен, но есть претензии")
            {
                details = "Заказ получен, но возникли проблемы. " + details;
            }

            Blockchainblock newBlock = CreateBlock(blockchainId, oldBlock?.Vendorid ?? 0, oldBlock?.Clientid ?? 0,
                oldBlock?.Transporterid ?? 0, userId, oldBlock?.Batchid ?? "", "Сделка закрыта", oldBlock?.Location ?? "",
                oldBlock?.Hash ?? "", oldBlock?.Carnumber ?? "", details);
            _supplyTrackingContext.Blockchainblocks.Add(newBlock);
            await _supplyTrackingContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CreateBlockhain([FromBody] CreateBlockchainRequestWithBatch requestWithBatch)
        {
            if (requestWithBatch == null || requestWithBatch.Request == null || requestWithBatch.Batch == null)
            {
                return BadRequest("Request body is null or incomplete");
            }

            var request = requestWithBatch.Request;
            var batch = requestWithBatch.Batch;
            var publicKey = requestWithBatch.Request.PublicKey;
            string signature = requestWithBatch.Request.Signature;

            Blockchainid blockchainid = new Blockchainid { Blockchainid1 = request.BatchId, Usergroupid = request.UserGroupId };
            _supplyTrackingContext.Blockchainids.Add(blockchainid);

            blockchain = new List<Blockchainblock>();
            difficulty = 4;
            int vendorId = _supplyTrackingContext.Vendorproducts.FirstOrDefault(v => v.Productid == batch.Products.FirstOrDefault().ProductId)?.Vendorid ?? 0;
            int clientId = _supplyTrackingContext.Clients.FirstOrDefault(c => c.Userid == request.UserId)?.Clientid ?? 0;
            CreateGenesisBlock(request.BatchId, request.UserId, request.DetailsOrder, vendorId, clientId);

            foreach (var block in blockchain)
            {
                _supplyTrackingContext.Blockchainblocks.Add(block);
            }

            _supplyTrackingContext.SaveChanges();
            return RedirectToAction("Index", new { selectedVendor = 1 });
        }

        private void CreateGenesisBlock(string BatchId, int UserId, string DetailsOrder, int VendorId, int ClientId)
        {
            string blockchainId = BatchId;
            int vendorId = VendorId;
            int clientId = ClientId;
            int transporterId = 0;
            int userId = UserId;
            string batchId = BatchId;
            string eventType = "Поиск транспортировщика";
            string location = _supplyTrackingContext.Clients.FirstOrDefault(c => c.Userid == userId)?.Address ?? "";
            string previousHash = "0";
            string carNumber = "0";
            string eventDetails = DetailsOrder;
            Blockchainblock genesisBlock = CreateBlock(blockchainId, vendorId, clientId, transporterId, userId, batchId, eventType, location, previousHash, carNumber, eventDetails);
            blockchain.Add(genesisBlock);
        }

        private Blockchainblock CreateBlock(string blockchainId, int vendorId, int clientId, int transporterId, int userId, string batchId, string eventType, string location, string previousHash, string carNumber, string eventDetails)
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
				Timestampblock = new DateTime(DateTime.Now.Ticks / 10000 * 10000, DateTimeKind.Unspecified),
				Nonce = 0,
                Signature = "",
                Publickey = "",
            };
			MineBlock(block);
			string dataToSignJson = JsonConvert.SerializeObject(block);
			var (publicKey, signature) = GenerateKeysAndSignData(dataToSignJson);
            block.Publickey = publicKey;
            block.Signature = signature;
			return block;
        }

		private (string publicKey, string signature) GenerateKeysAndSignData(string data)
		{
			using (RSA rsa = RSA.Create(2048))
			{
				// Экспорт публичного ключа в формате JSON
				var parameters = rsa.ExportParameters(false);
				var publicKey = JsonConvert.SerializeObject(new
				{
					n = Convert.ToBase64String(parameters.Modulus),
					e = Convert.ToBase64String(parameters.Exponent)
				});

				// Экспорт приватного ключа (если нужно)
				var privateKey = JsonConvert.SerializeObject(rsa.ExportParameters(true));

				// Подпись данных
				byte[] dataBytes = Encoding.UTF8.GetBytes(data);
				byte[] signatureBytes = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
				string signature = Convert.ToBase64String(signatureBytes);

				return (publicKey, signature);
			}
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
            try
            {
                while (!block.Hash!.StartsWith(target))
                {
                    block.Nonce++;
                    block.Hash = CalculateHash(block);
                    if (block.Nonce % 100000 == 0)
                    {
                        Console.WriteLine($"Майнинг... nonce: {block.Nonce}");
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                Console.WriteLine("Майнинг прерван пользователем.");
            }
        }

        private int GetCurrentUserId()
        {
            if (User.Identity!.IsAuthenticated)
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            return 0;
        }
    }
}
