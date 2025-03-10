﻿using MySqlConnector;
using ServerCode.Models;
using static Repositories.DBConfig;

namespace Repositories
{
    public class PlayerItemRepostory : IRepository<PlayerItemInfo>
    {

        public async Task<bool> AddAsync(PlayerItemInfo itemInfo, MySqlConnection connection, MySqlTransaction transaction)
        {
            if (itemInfo.quantity <= 0) return false;

            var cmd = new MySqlCommand(Queries.InsertItem, connection, transaction);
            cmd.Parameters.AddWithValue("@playerId", itemInfo.playerId);
            cmd.Parameters.AddWithValue("@itemId", itemInfo.itemId);
            cmd.Parameters.AddWithValue("@quantity", itemInfo.quantity);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }
        public async Task<bool> DeleteAsync(PlayerItemInfo itemInfo, MySqlConnection connection, MySqlTransaction transaction)
        {
            var cmd = new MySqlCommand(Queries.DeleteItem, connection, transaction);
            cmd.Parameters.AddWithValue("@playerId", itemInfo.playerId);
            cmd.Parameters.AddWithValue("@itemId", itemInfo.itemId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public Task<List<PlayerItemInfo>> GetAllItemsAsync(MySqlConnection connection, MySqlTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public async Task<PlayerItemInfo?> GetItemByPrimaryKeysAsync(PlayerItemInfo itemInfo, MySqlConnection connection, MySqlTransaction transaction)
        {
            var cmd = new MySqlCommand(Queries.GetPlayerItemDataInfo, connection, transaction);
            cmd.Parameters.AddWithValue("@playerId", itemInfo.playerId);
            cmd.Parameters.AddWithValue("@itemId", itemInfo.itemId);

            var table = await cmd.ExecuteReaderAsync();
            PlayerItemInfo? info = null;
            if(await table.ReadAsync())
            {
                info = new PlayerItemInfo
                {
                    itemId = table.GetInt32(table.GetOrdinal(ITEM_ID)),
                    playerId = table.GetString(table.GetOrdinal(PLAYER_ID)),
                    quantity = table.GetInt32(table.GetOrdinal(QUANTITY))
                };
            }
            await table.CloseAsync();
            return info;
        }

        public async Task<bool> UpdateAsync(PlayerItemInfo itemInfo, MySqlConnection connection, MySqlTransaction transaction)
        {
            var cmd = new MySqlCommand(Queries.UpdateItemQuantity, connection, transaction);
            cmd.Parameters.AddWithValue("@playerId", itemInfo.playerId);
            cmd.Parameters.AddWithValue("@itemId", itemInfo.itemId);
            cmd.Parameters.AddWithValue("@quantity", itemInfo.quantity);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}
