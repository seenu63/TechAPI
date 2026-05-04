using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace TechAPI.DBContext
{
    public interface IDBContextService
    {
        Task<List<IEnumerable<dynamic>>> Execute(string spName, object? parameters = null);       
    }

    public class DBContext : IDBContextService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _Configuration;
        public DBContext(IConfiguration oConfiguration)
        {
            _Configuration = oConfiguration;
           // _connectionString = "Server=DESK-333\\SQLEXPRESS;Database=CORE_CHDR;Trusted_Connection=True;Trust Server Certificate=True;";
        }                      

        public string GetConnection()
        {
            return _Configuration.GetConnectionString("DBConnection");
        }
        public async Task<List<IEnumerable<dynamic>>> Execute(string spName, object? parameters = null)
        {
            
            await using var connection = new SqlConnection(GetConnection());

          try
            {
                using var multi = await connection.QueryMultipleAsync(
                spName,
                parameters,
                commandType: CommandType.StoredProcedure);
            
                var results = new List<IEnumerable<dynamic>>();

                while (!multi.IsConsumed)
                {
                    var table = await multi.ReadAsync();
                    results.Add(table);
                }

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
            return null;
        }
    }
}
