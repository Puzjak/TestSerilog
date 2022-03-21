using Microsoft.Data.SqlClient;
using System.Data;
using TestSerilog.Dto;

namespace TestSerilog.Services
{
    public interface ILogQueryService
    {
        IEnumerable<LogDto> Query(LogQueryDto query);
    }
    public class LogQueryService : ILogQueryService
    {
        private readonly IConfiguration _configuration;

        public LogQueryService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IEnumerable<LogDto> Query(LogQueryDto query)
        {
            string connectionString = _configuration["Serilog:WriteTo:0:Args:connectionString"];
            string tableName = _configuration["Serilog:WriteTo:0:Args:tableName"];
            using SqlConnection connection = new(connectionString);
            List<LogDto> result = new();
            try
            {
                List<SqlParameter> sqlParams = new();

                string sqlQuery = @$"SELECT * FROM {tableName}";

                if (query is not null && !string.IsNullOrWhiteSpace(query.PropertyName) && !string.IsNullOrWhiteSpace(query.PropertyValue))
                {
                    sqlQuery += " WHERE Properties.value('(/properties/property[@key=sql:variable(\"@PropertyName\")])[1]', 'nvarchar(max)') = @PropertyValue";
                    sqlParams.Add(new SqlParameter("PropertyName", query.PropertyName));
                    sqlParams.Add(new SqlParameter("PropertyValue", query.PropertyValue));
                }
                sqlQuery += " ORDER BY TimeStamp";

                SqlCommand sqlCommand = new(sqlQuery, connection);

                foreach (var param in sqlParams)
                {
                    sqlCommand.Parameters.Add(param);
                }
                
                connection.Open();
                using var reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new LogDto
                    {
                        Exception = reader.GetNullableString("Exception"),
                        Id = reader.GetInt32("Id"),
                        Level = reader.GetNullableString("Level"),
                        Message = reader.GetNullableString("Message"),
                        MessageTemplate = reader.GetNullableString("MessageTemplate"),
                        TimeStamp = reader.GetDateTime("TimeStamp")
                    });
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            return result;
        }
        /*
         
            Raw SQL query which filters log by desired property will look like this:

            DECLARE
	            @PropertyName NVARCHAR(MAX) = 'CorrelationId',
	            @PropertyValue NVARCHAR(MAX) = '62d17049-25d4-4707-8f58-58f693e59924'

            SELECT 
	            * 
            FROM 
	            ApplicationLog 
            WHERE 
	            Properties.value('(/properties/property[@key=sql:variable("@PropertyName")])[1]', 'nvarchar(max)') = @PropertyValue 
            ORDER 
	            BY TimeStamp
         
         */
    }

    public static class Extensions
    {
        public static string GetNullableString(this SqlDataReader reader, string colName)
        {
            if (!reader.IsDBNull(colName))
                return reader.GetString(colName);
            return null;
        }
    }
}
