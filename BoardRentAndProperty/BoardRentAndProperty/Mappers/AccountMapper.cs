namespace BoardRentAndProperty.Mappers
{
    using System;
    using System.Collections.Generic;
    using BoardRentAndProperty.Models;
    using BoardRentAndProperty.DataTransferObjects;
    using Microsoft.Data.SqlClient;

    public class AccountMapper : IMapper<Account, RegisterDataTransferObject>
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
                PamUserId = dataReader.IsDBNull(dataReader.GetOrdinal("PamUserId")) ? (int?)null : dataReader.GetInt32(dataReader.GetOrdinal("PamUserId")),
                Roles = new List<Role>()
            };
        }

        public Account ToModel(RegisterDataTransferObject sourceDataTransferObject)
        {
            return new Account
            {
                Id = Guid.NewGuid(),
                Username = sourceDataTransferObject.Username,
                Email = sourceDataTransferObject.Email,
                DisplayName = sourceDataTransferObject.DisplayName,
                PasswordHash = sourceDataTransferObject.Password,

                PhoneNumber = sourceDataTransferObject.PhoneNumber,
                StreetName = sourceDataTransferObject.StreetName,
                StreetNumber = sourceDataTransferObject.StreetNumber,
                City = sourceDataTransferObject.City,
                Country = sourceDataTransferObject.Country,

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsSuspended = false,
                Roles = new List<Role>()
            };
        }

        public RegisterDataTransferObject ToDTO(Account sourceModel)
        {
            return new RegisterDataTransferObject
            {
                Username = sourceModel.Username,
                Email = sourceModel.Email,
                DisplayName = sourceModel.DisplayName
            };
        }
    }
}