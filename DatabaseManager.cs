using Npgsql;
using System.Data;

namespace Darts_for_people
{
    public class DatabaseManager
    {
        private string _connectionString;

        public DatabaseManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Отправляет запрос в БД.
        /// </summary>
        /// <param name="sql">SQL хапрос в БД.</param>
        /// <returns>Таблица с полученными данными.</returns>
        public async Task<DataTable> ExecuteQueryAsync(string sql)
        {
            DataTable dataTable = new();

            await using (NpgsqlConnection connection = new(_connectionString))
            {
                await connection.OpenAsync();
                await using (NpgsqlCommand command = new(sql, connection))
                {
                    await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        dataTable.Load(reader);
                    }
                }
            }

            return dataTable;
        }

    }
}