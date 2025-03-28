﻿using MySqlConnector;
using Repositories;
using ServerCode.Models;
using static Repositories.DBConfig;

namespace DataAccessLayer.Repositories
{
    public interface IPlayerInfoRepository : IRepository<PlayerInfo>
    {

    }
    public class PlayerInfoRepository : IPlayerInfoRepository
    {

        public async Task<bool> AddAsync(PlayerInfo playerInfo, MySqlConnection connection, MySqlTransaction transaction)
        {
            MySqlCommand command = new MySqlCommand(
                $"INSERT INTO {PLAYER_LOGIN_DATA_TABLE} ({PLAYER_ID},{PASSWORD})" +
                $" VALUES (@playerId,@password)", connection, transaction);
            command.Parameters.AddWithValue("@playerId", playerInfo.id);
            command.Parameters.AddWithValue("@password", playerInfo.password);
            var table = await command.ExecuteNonQueryAsync();
            return table == 1;
        }
        public Task<bool> DeleteWithPrimaryKeysAsync(PlayerInfo playerInfo, MySqlConnection connection, MySqlTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public Task<List<PlayerInfo>> GetAllItemsAsync(MySqlConnection connection)
        {
            throw new NotImplementedException();
        }

        public async Task<PlayerInfo> GetItemByPrimaryKeysAsync(PlayerInfo playerInfo, MySqlConnection connection)//정보만 가지고 오고 로그인은 DBManager에서 구현
        {
            MySqlCommand command = new MySqlCommand(
                $"SELECT * FROM {PLAYER_LOGIN_DATA_TABLE}" +
                $" WHERE {PLAYER_ID} = @playerId", connection);
            command.Parameters.AddWithValue("@playerId", playerInfo.id);
            var table = await command.ExecuteReaderAsync();
            PlayerInfo info = new PlayerInfo();
            while (await table.ReadAsync())
            {
                string playerId = table.GetString(table.GetOrdinal(PLAYER_ID));
                string password = table.GetString(table.GetOrdinal(PASSWORD));
                info.id = playerId;
                info.password = password;
            }
            await table.CloseAsync();
            return info;
        }

        public Task<bool> UpdateAsync(PlayerInfo entity, MySqlConnection connection, MySqlTransaction transaction)
        {
            throw new NotImplementedException();
        }
    }

}
