using App.Core.Interfaces;
using App.Dtos.ResponseModel;
using App.Dtos.ResquestModel;
using App.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace App.Core.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<WalletService> _logger;

        public WalletService(
            IWalletRepository walletRepository,
            INotificationService notificationService,
            ILogger<WalletService> logger)
        {
            _walletRepository = walletRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ModelResponse<Wallet>> DebitWallet(string userId, decimal amount)
        {
            _logger.LogInformation("Attempting to debit wallet for user {UserId} with amount {Amount}", userId, amount);

            try
            {
                if (amount <= 0)
                {
                    _logger.LogWarning("Invalid debit amount: {Amount}", amount);
                    return new ModelResponse<Wallet> { Message = "Invalid amount" };
                }

                var wallet = await _walletRepository.GetWalletByUserId(userId);
                if (wallet == null)
                {
                    _logger.LogWarning("Wallet not found for user {UserId}", userId);
                    await _notificationService.SendEmail(new EmailDto { Message = "Insuficient fund in wallet " });
                    return new ModelResponse<Wallet> { Message = "Wallet not found" };
                }

                if (amount > wallet.Balance)
                {
                    _logger.LogWarning("Insufficient funds for user {UserId}. Requested: {Amount}, Available: {Balance}", userId, amount, wallet.Balance);
                    return new ModelResponse<Wallet> { Message = "Insufficient balance" };
                }

                wallet.Balance -= amount;
                var walletResponse = await _walletRepository.UpdateWallet(wallet);

                _logger.LogInformation("Successfully debited wallet for user {UserId}. New Balance: {Balance}", userId, wallet.Balance);
                return new ModelResponse<Wallet> { Data = wallet, Message = "Wallet debited", Status = true };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error debiting wallet for user {UserId}", userId);
                return new ModelResponse<Wallet> { Message = e.Message };
            }
        }

        public async Task<ModelResponse<decimal>> CreditWallet(string userId, decimal amount)
        {
            _logger.LogInformation("Attempting to credit wallet for user {UserId} with amount {Amount}", userId, amount);

            try
            {
                if (amount <= 0)
                {
                    _logger.LogWarning("Invalid credit amount: {Amount}", amount);
                    return new ModelResponse<decimal> { Message = "Invalid amount" };
                }

                var wallet = await _walletRepository.GetWalletByUserId(userId);
                if (wallet == null)
                {
                    _logger.LogWarning("Wallet not found for user {UserId}", userId);
                    return new ModelResponse<decimal> { Message = "Wallet not found" };
                }

                wallet.Balance += amount;
                var walletResponse = await _walletRepository.UpdateWallet(wallet);

                _logger.LogInformation("Successfully credited wallet for user {UserId}. New Balance: {Balance}", userId, wallet.Balance);
                return new ModelResponse<decimal> { Data = amount, Message = "Wallet credited", Status = true };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error crediting wallet for user {UserId}", userId);
                return new ModelResponse<decimal> { Message = e.Message };
            }
        }
    }
}
