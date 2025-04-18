﻿using AutoMapper;
using MySqlConnector;
using Repositories;
using ServerCode.DAO;

namespace BusinessLayer.Services
{
    public class ItemInfoService : Service
    {
        public ItemInfoService(RepositoryManager repo, IMapper mapper, string dbAddress) : base(repo, mapper, dbAddress)
        {
        }

        public async Task<bool> AddItemInfo(ItemInfo itemInfo)
        {
            await using MySqlConnection connection = new MySqlConnection(_dbAddress);
            await connection.OpenAsync();
            await using MySqlTransaction transaction = await connection.BeginTransactionAsync();

            try
            {
                bool success = await _repositoryManager.ItemInfos.AddAsync(itemInfo, connection, transaction);
                await transaction.CommitAsync();
                return success;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }


        }
        public bool RemoveItemInfo(string itemName)
        {
            return true;
        }
        public List<ItemInfo> GetItemInfos()
        {
            return null!;
        }
    }
}
