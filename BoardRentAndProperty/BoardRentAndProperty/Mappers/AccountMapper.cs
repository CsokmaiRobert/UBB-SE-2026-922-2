namespace BoardRentAndProperty.Mappers
{
    using System.Collections.Generic;
    using BoardRentAndProperty.Models;
    using Microsoft.Data.SqlClient;

    public class AccountMapper
    {
        public Account FromReader(SqlDataReader dataReader)
        {
            return new Account
            {
                Id = dataReader.GetGuid(dataReader.GetOrdinal("Id")),
                Username = dataReader.GetString(dataReader.GetOrdinal("Username")),
                DisplayName = dataReader.GetString(dataReader.GetOrdinal("DisplayName")),
                Email = dataReader.GetString(dataReader.GetOrdinal("Email")),
                PasswordHash = dataReader.GetString(dataReader.GetOrdinal("PasswordHash")),
                PhoneNumber = dataReader.IsDBNull(dataReader.GetOrdinal("PhoneNumber")) ? null : dataReader.GetString(dataReader.GetOrdinal("PhoneNumber")),
                AvatarUrl = dataReader.IsDBNull(dataReader.GetOrdinal("AvatarUrl")) ? null : dataReader.GetString(dataReader.GetOrdinal("AvatarUrl")),
                IsSuspended = dataReader.GetBoolean(dataReader.GetOrdinal("IsSuspended")),
                CreatedAt = dataReader.GetDateTime(dataReader.GetOrdinal("CreatedAt")),
                UpdatedAt = dataReader.GetDateTime(dataReader.GetOrdinal("UpdatedAt")),
                StreetName = dataReader.IsDBNull(dataReader.GetOrdinal("StreetName")) ? null : dataReader.GetString(dataReader.GetOrdinal("StreetName")),
                StreetNumber = dataReader.IsDBNull(dataReader.GetOrdinal("StreetNumber")) ? null : dataReader.GetString(dataReader.GetOrdinal("StreetNumber")),
                Country = dataReader.IsDBNull(dataReader.GetOrdinal("Country")) ? null : dataReader.GetString(dataReader.GetOrdinal("Country")),
                City = dataReader.IsDBNull(dataReader.GetOrdinal("City")) ? null : dataReader.GetString(dataReader.GetOrdinal("City")),
                Roles = new List<Role>(),
            };
        }
    }
}
