﻿using AutoMapper;
using MySqlConnector;
using Repositories;
using ServerCode.DTO;
using ServerCode.DAO;

namespace BusinessLayer.Services
{
    public class AuctionService : Service
    {
        public AuctionService(RepositoryManager repo, IMapper mapper, string dbAddress) : base(repo, mapper, dbAddress)
        {
        }

        public async Task<bool> AddItemToAuction(AuctionItemDTO auctionItemDTO)
        {
            await using var connection = new MySqlConnection(_dbAddress);
            await connection.OpenAsync();
            AuctionItemDAO auctionItemInfo = _mapper.Map<AuctionItemDTO, AuctionItemDAO>(auctionItemDTO);
            var playerItemInfo = new PlayerItemDAO() { playerId = auctionItemInfo.playerId, itemName = auctionItemInfo.itemName, quantity = 0 };

            playerItemInfo = await _repositoryManager.PlayerItems.GetItemByPrimaryKeysAsync(playerItemInfo, connection);
            var remainItemInfo = await _repositoryManager.AuctionItems.GetItemByPrimaryKeysAsync(auctionItemInfo, connection);

            if (playerItemInfo == null)
                return false;

            int remainQuantity = playerItemInfo.quantity - auctionItemInfo.quantity;
            if (remainQuantity < 0)
            {
                await connection.CloseAsync();
                return false;
            }
            playerItemInfo.quantity = remainQuantity;
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                bool success = await _repositoryManager.PlayerItems.UpdateAsync(playerItemInfo,connection,transaction);

                if (remainItemInfo == null && auctionItemInfo.quantity > 0)
                {
                    success &= await _repositoryManager.AuctionItems.AddAsync(auctionItemInfo, connection, transaction);
                    if (success) await transaction.CommitAsync();
                    else await transaction.RollbackAsync();
                    return success;
                }

                int quantity = remainItemInfo.quantity + auctionItemInfo.quantity;
                if (quantity < 0)
                    return false;
                remainItemInfo.quantity = quantity;
                
                success &= await _repositoryManager.AuctionItems.UpdateAsync(remainItemInfo, connection, transaction);
                Console.WriteLine("Auction Update: " + success);
                if (success) await transaction.CommitAsync();
                else await transaction.RollbackAsync();
                return success;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> PurchaseItemInAuction(BuyerDTO buyerInfo)
        {
            try
            {
                await using MySqlConnection connection = new MySqlConnection(_dbAddress);
                await connection.OpenAsync();

                PlayerDataDAO buyer = new() { playerId = buyerInfo.buyerId, gold = 0 };
                buyer = await _repositoryManager.PlayerData.GetItemByPrimaryKeysAsync(buyer, connection);

                AuctionItemDAO? auctionItem = buyerInfo.itemInfo;
                auctionItem = await _repositoryManager.AuctionItems.GetItemByPrimaryKeysAsync(auctionItem, connection);

                if (buyer == null || auctionItem == null)
                {
                    await connection.CloseAsync();
                    return false;
                }

                if (buyer.gold < buyerInfo.NeededMoney || auctionItem.quantity < buyerInfo.buyCount)
                {
                    await connection.CloseAsync();
                    return false;
                }

                //돈 빼고 아이템 추가하고 옥션에서 아이템 빼고 리턴
                bool success = true;


                PlayerDataDAO sellerInfo = new() { playerId = auctionItem.playerId, gold = 0 };
                sellerInfo = await _repositoryManager.PlayerData.GetItemByPrimaryKeysAsync(sellerInfo, connection);

                PlayerItemDAO itemInfo = new() { itemName = auctionItem.itemName, playerId = buyer.playerId, quantity = buyerInfo.buyCount };
                var remainItem = await _repositoryManager.PlayerItems.GetItemByPrimaryKeysAsync(itemInfo, connection);

                await using MySqlTransaction transaction = await connection.BeginTransactionAsync();

                success &= await _repositoryManager.PlayerItems.CheckConditionAndChangePlayerItem(itemInfo, remainItem, connection, transaction);

                sellerInfo.gold += buyerInfo.NeededMoney;
                success &= await _repositoryManager.PlayerData.UpdateAsync(sellerInfo, connection, transaction);

                buyer.gold -= buyerInfo.NeededMoney;
                success &= await _repositoryManager.PlayerData.UpdateAsync(buyer, connection, transaction);

                int remain = auctionItem.quantity -= buyerInfo.buyCount;
                if (remain == 0)
                    success &= await _repositoryManager.AuctionItems.DeleteWithPrimaryKeysAsync(auctionItem, connection, transaction);
                else
                    success &= await _repositoryManager.AuctionItems.UpdateAsync(auctionItem, connection, transaction);

                if (success)
                    await transaction.CommitAsync();
                else
                    await transaction.RollbackAsync();

                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
        public async Task<bool> CancelAuctionItem(string playerId,string itemName,int pricePerUnit)
        {
            await using MySqlConnection connection = new MySqlConnection(_dbAddress);
            await connection.OpenAsync();
            AuctionItemDAO auctionItemInfo = new() { itemName = itemName, playerId = playerId, pricePerUnit = pricePerUnit };
            auctionItemInfo = await _repositoryManager.AuctionItems.GetItemByPrimaryKeysAsync(auctionItemInfo, connection);
            if (auctionItemInfo == null)
            {
                await connection.CloseAsync();
                return false;
            }
            PlayerItemDAO playerItemInfo = new(auctionItemInfo);
            PlayerItemDAO remainItem = await _repositoryManager.PlayerItems.GetItemByPrimaryKeysAsync(playerItemInfo, connection);
            using MySqlTransaction transaction = await connection.BeginTransactionAsync();
            try
            {
                bool success = true;
                success &= await _repositoryManager.AuctionItems.DeleteWithPrimaryKeysAsync(auctionItemInfo, connection, transaction);
                success &= await _repositoryManager.PlayerItems.CheckConditionAndChangePlayerItem(playerItemInfo, remainItem, connection, transaction);

                if (success)
                    await transaction.CommitAsync();
                else
                    await transaction.RollbackAsync();
                return success;
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex);
                await transaction.RollbackAsync();
                return false;
            }
        }
        public async Task<List<AuctionItemDTO>> GetAuctionItemByItemName(string itemName)
        {
            await using MySqlConnection connection = new MySqlConnection(_dbAddress);
            await connection.OpenAsync();
            var datas = await _repositoryManager.AuctionItems.GetItemsByItemName(itemName, connection);
            List<AuctionItemDTO> auctionItems = new List<AuctionItemDTO>();
            datas.ForEach(item => auctionItems.Add(_mapper.Map<AuctionItemDAO, AuctionItemDTO>(item)));
            return auctionItems;
        }
        public async Task<List<AuctionItemDTO>> GetAuctionnItemByPlayerId(string playerId)
        {
            await using MySqlConnection connection = new MySqlConnection(_dbAddress);
            await connection.OpenAsync();
            var datas = await _repositoryManager.AuctionItems.GetItemsByPlayerId(playerId, connection);
            List<AuctionItemDTO> dtos = new List<AuctionItemDTO>();
            datas.ForEach(item => dtos.Add(_mapper.Map<AuctionItemDAO, AuctionItemDTO>(item)));
            return dtos;
        }
    }
}
