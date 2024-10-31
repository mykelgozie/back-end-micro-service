using App.Core.Interfaces;
using App.Dtos.ResponseModel;
using App.Dtos.ResquestModel;
using App.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Core.Services
{
    public class ElectricityBillService : IElectricityBillService
    {
        private readonly IElectricityBillRepository _electricityBillRepository;
        private readonly IProducerService _producerService;
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletService _walletService;
        private readonly ILogger<ElectricityBillService> _logger;

        public ElectricityBillService(
            IElectricityBillRepository electricityBillRepository,
            IProducerService producerService,
            IWalletRepository walletRepository,
            IWalletService walletService,
            ILogger<ElectricityBillService> logger)
        {
            _electricityBillRepository = electricityBillRepository;
            _producerService = producerService;
            _walletRepository = walletRepository;
            _walletService = walletService;
            _logger = logger;
        }

        public async Task<ModelResponse<ElectricityBill>> GetPendingBillByVerificationRef(string verificationRef)
        {
            _logger.LogInformation("Retrieving pending bill with verification reference: {VerificationRef}", verificationRef);

            var bill = await _electricityBillRepository.GetElectricityBillByVerificationRef(verificationRef);
            if (bill == null)
            {
                _logger.LogWarning("No bill found for verification reference: {VerificationRef}", verificationRef);
                return new ModelResponse<ElectricityBill> { Message = "no bill found" };
            }

            if (bill.Status != PaymentStatus.Pending)
            {
                _logger.LogInformation("Payment has already been made for verification reference: {VerificationRef}", verificationRef);
                return new ModelResponse<ElectricityBill> { Message = "Payment has been made" };
            }

            _logger.LogInformation("Bill successfully retrieved for verification reference: {VerificationRef}", verificationRef);
            return new ModelResponse<ElectricityBill> { Data = bill, Message = "Bill successfully retrieved", Status = true };
        }

        public async Task<ModelResponse<ElectricityBill>> CreateBill(BillDto billDto)
        {
            _logger.LogInformation("Creating new bill for user: {UserId}", billDto.UserId);

            try
            {
                var electricBill = new ElectricityBill()
                {
                    Amount = billDto.Amount,
                    Status = PaymentStatus.Pending,
                    VerificationRef = GenerateRandomChar(),
                    UserId = billDto.UserId,
                };

                await _producerService.PublishBillAsync(electricBill);

                _logger.LogInformation("New bill created with verification reference: {VerificationRef}", electricBill.VerificationRef);
                return new ModelResponse<ElectricityBill> { Data = electricBill, Status = true, Message = "New bill created" };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating bill for user: {UserId}", billDto.UserId);
                return new ModelResponse<ElectricityBill> { Status = false, Message = "Error cannot process bill" };
            }
        }

        public string GenerateRandomChar(int length = 10)
        {
            _logger.LogInformation("Generating random verification reference of length: {Length}", length);

            Random random = new Random();
            return new string(Enumerable.Range(0, length).Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"[random.Next(62)]).ToArray());
        }

        public async Task<ModelResponse<ElectricityBill>> VendBill(string verificationRef)
        {
            _logger.LogInformation("Vending bill with verification reference: {VerificationRef}", verificationRef);

            var billResponse = await GetPendingBillByVerificationRef(verificationRef);
            if (!billResponse.Status)
            {
                _logger.LogWarning("Failed to retrieve pending bill for verification reference: {VerificationRef}", verificationRef);
                return billResponse;
            }

            var pendingBill = billResponse.Data;

            var wallet = await _walletRepository.GetWalletByUserId(pendingBill.UserId);
            if (wallet == null)
            {
                _logger.LogWarning("Wallet not found for user: {UserId}", pendingBill.UserId);
                return new ModelResponse<ElectricityBill> { Message = "wallet not found" };
            }

            _logger.LogInformation("Publishing vend bill event for verification reference: {VerificationRef}", verificationRef);
            await _producerService.PublishVendBillAsync(pendingBill);

            _logger.LogInformation("Payment processing started for bill with verification reference: {VerificationRef}", verificationRef);
            return new ModelResponse<ElectricityBill> { Message = "Processing payment; token would be sent", Status = true };
        }
    }
}
