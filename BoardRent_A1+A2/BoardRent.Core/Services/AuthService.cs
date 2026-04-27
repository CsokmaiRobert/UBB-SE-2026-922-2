namespace BoardRent.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using BoardRent.Data;
    using BoardRent.DataTransferObjects;
    using BoardRent.Domain;
    using BoardRent.Repositories;
    using BoardRent.Utils;

    public class AuthService : IAuthService
    {
        private readonly IUserRepository userRepository;
        private readonly IFailedLoginRepository failedLoginRepository;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly ISessionContext sessionContext;

        public AuthService(
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

        public async Task<ServiceResult<bool>> RegisterAsync(RegisterDataTransferObject registrationRequest)
        {
            // Validările rămân în mare parte la fel, dar folosim numele întregi
            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.userRepository.SetUnitOfWork(unitOfWork);

                User existingUserByUsername = await this.userRepository.GetByUsernameAsync(registrationRequest.Username);
                if (existingUserByUsername != null)
                {
                    return ServiceResult<bool>.Fail("Username|Username is already taken.");
                }

                User newUser = new User
                {
                    Id = Guid.NewGuid(),
                    DisplayName = registrationRequest.DisplayName,
                    Username = registrationRequest.Username,
                    Email = registrationRequest.Email,
                    PasswordHash = PasswordHasher.HashPassword(registrationRequest.Password),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsSuspended = false
                };

                await this.userRepository.AddAsync(newUser);
                await this.userRepository.AddRoleAsync(newUser.Id, "Standard User");

                this.sessionContext.Populate(newUser, "Standard User");
                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<UserProfileDataTransferObject>> LoginAsync(LoginDataTransferObject loginRequest)
        {
            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.userRepository.SetUnitOfWork(unitOfWork);
                this.failedLoginRepository.SetUnitOfWork(unitOfWork);

                User userEntity = await this.userRepository.GetByUsernameAsync(loginRequest.UsernameOrEmail);
                if (userEntity == null)
                {
                    userEntity = await this.userRepository.GetByEmailAsync(loginRequest.UsernameOrEmail);
                }

                if (userEntity == null)
                {
                    return ServiceResult<UserProfileDataTransferObject>.Fail("Invalid username or password.");
                }

                if (userEntity.IsSuspended)
                {
                    return ServiceResult<UserProfileDataTransferObject>.Fail("This account has been suspended.");
                }

                if (!PasswordHasher.VerifyPassword(loginRequest.Password, userEntity.PasswordHash))
                {
                    await this.failedLoginRepository.IncrementAsync(userEntity.Id);
                    return ServiceResult<UserProfileDataTransferObject>.Fail("Invalid username or password.");
                }

                await this.failedLoginRepository.ResetAsync(userEntity.Id);

                string primaryRole = userEntity.Roles?.FirstOrDefault()?.Name ?? "Standard User";
                this.sessionContext.Populate(userEntity, primaryRole);

                UserProfileDataTransferObject profileDto = new UserProfileDataTransferObject
                {
                    Id = userEntity.Id,
                    Username = userEntity.Username,
                    DisplayName = userEntity.DisplayName,
                    Email = userEntity.Email,
                    Role = new RoleDataTransferObject { Name = primaryRole }
                };

                return ServiceResult<UserProfileDataTransferObject>.Ok(profileDto);
            }
        }

        public async Task<ServiceResult<bool>> LogoutAsync()
        {
            this.sessionContext.Clear();
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<string>> ForgotPasswordAsync()
        {
            return ServiceResult<string>.Ok("Please contact the Administrator at admin@boardrent.com.");
        }
    }
}