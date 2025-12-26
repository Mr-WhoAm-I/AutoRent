using Microsoft.Data.SqlClient;

namespace CarRental.DAL.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=АрендаАвто;User ID=RentalAdmin;Password=58hehehe58;Encrypt=True;TrustServerCertificate=True;";

        protected SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}