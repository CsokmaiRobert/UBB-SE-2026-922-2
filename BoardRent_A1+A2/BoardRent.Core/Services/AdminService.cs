namespace BoardRent.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BoardRent.Data;
    using BoardRent.DataTransferObjects;
    using BoardRent.Domain;
    using BoardRent.Repositories;
    using BoardRent.Utils;

    public class AdminService : IAdminService
    {
        private readonly IUserRepository userRepository;
        private readonly IFailedLoginRepository failedLoginRepository;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly ISessionContext sessionContext;

        public AdminService(
            IUserRepository userRepository,
            IFailedLoginRepository failedLoginRepository,
            IUnitOfWorkFactory unitOfWorkFactory,
            ISessionContext sessionContext)
        {
            this.userRepository = userRepository;
            this.failedLoginRepository = failedLoginRepository;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.sessionContext = sessionContext;
        }

        private bool IsAuthorized()
        {
            return this.sessionContext.IsLoggedIn && this.sessionContext.Role == "Administrator";
        }

        public async Task<ServiceResult<List<UserProfileDataTransferObject>>> GetAllUsersAsync(int pageNumber, int pageSize)
        {
            if (!this.IsAuthorized())
            {
                return ServiceResult<List<UserProfileDataTransferObject>>.Fail("Unauthorized access.");
            }

            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.userRepository.SetUnitOfWork(unitOfWork);
                this.failedLoginRepository.SetUnitOfWork(unitOfWork);

                List<User> userEntities = await this.userRepository.GetAllAsync(pageNumber, pageSize);

                List<UserProfileDataTransferObject> userProfileDtos = new List<UserProfileDataTransferObject>();

                foreach (User userEntity in userEntities)
                {
                    Role firstRole = userEntity.Roles?.FirstOrDefault();
                    FailedLoginAttempt failedAttempt = await this.failedLoginRepository.GetByUserIdAsync(userEntity.Id);

                    bool isLocked = failedAttempt != null
                        && failedAttempt.LockedUntil.HasValue
                        && failedAttempt.LockedUntil.Value > DateTime.UtcNow;

                    userProfileDtos.Add(new UserProfileDataTransferObject
                    {
                        Id = userEntity.Id,
                        Username = userEntity.Username,
                        DisplayName = userEntity.DisplayName,
                        Email = userEntity.Email,
                        PhoneNumber = userEntity.PhoneNumber,
                        AvatarUrl = userEntity.AvatarUrl,
                        Role = new RoleDataTransferObject
                        {
                            Id = firstRole?.Id ?? Guid.Empty,
                            Name = firstRole?.Name ?? "Standard User"
                        },
                        IsSuspended = userEntity.IsSuspended,
                        IsLocked = isLocked,
                        Country = userEntity.Country,
                        City = userEntity.City,
                        StreetName = userEntity.StreetName,
                        StreetNumber = userEntity.StreetNumber
                    });
                }

                return ServiceResult<List<UserProfileDataTransferObject>>.Ok(userProfileDtos);
            }
        }

        public async Task<ServiceResult<bool>> SuspendUserAsync(Guid userId)
        {
            if (!this.IsAuthorized())
            {
                return ServiceResult<bool>.Fail("Unauthorized access.");
            }

            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.userRepository.SetUnitOfWork(unitOfWork);

                User userEntity = await this.userRepository.GetByIdAsync(userId);
                if (userEntity == null)
                {
                    return ServiceResult<bool>.Fail("User not found.");
                }

                userEntity.IsSuspended = true;
                await this.userRepository.UpdateAsync(userEntity);

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> UnsuspendUserAsync(Guid userId)
        {
            if (!this.IsAuthorized())
            {
                return ServiceResult<bool>.Fail("Unauthorized access.");
            }

            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.userRepository.SetUnitOfWork(unitOfWork);

                User userEntity = await this.userRepository.GetByIdAsync(userId);
                if (userEntity == null)
                {
                    return ServiceResult<bool>.Fail("User not found.");
                }

                userEntity.IsSuspended = false;
                await this.userRepository.UpdateAsync(userEntity);

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> ResetPasswordAsync(Guid userId, string newPassword)
        {
            if (!this.IsAuthorized())
            {
                return ServiceResult<bool>.Fail("Unauthorized access.");
            }

            const int MinimumPasswordLength = 6;
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < MinimumPasswordLength)
            {
                return ServiceResult<bool>.Fail("Password must be at least 6 characters long.");
            }

            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.userRepository.SetUnitOfWork(unitOfWork);

                User userEntity = await this.userRepository.GetByIdAsync(userId);
                if (userEntity == null)
                {
                    return ServiceResult<bool>.Fail("User not found.");
                }

                userEntity.PasswordHash = PasswordHasher.HashPassword(newPassword);
                await this.userRepository.UpdateAsync(userEntity);

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> UnlockAccountAsync(Guid userId)
        {
            if (!this.IsAuthorized())
            {
                return ServiceResult<bool>.Fail("Unauthorized access.");
            }

            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.failedLoginRepository.SetUnitOfWork(unitOfWork);

                await this.failedLoginRepository.ResetAsync(userId);

                return ServiceResult<bool>.Ok(true);
            }
        }
    }
}