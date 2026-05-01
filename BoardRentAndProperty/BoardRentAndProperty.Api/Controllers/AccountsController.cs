using System;
using System.IO;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoardRentAndProperty.Api.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountsController : ControllerBase
    {
        private const long MaximumAvatarUploadBytes = 5 * 1024 * 1024;

        private readonly IAccountService accountService;
        private readonly IAvatarStorageService avatarStorageService;

        public AccountsController(IAccountService accountService, IAvatarStorageService avatarStorageService)
        {
            this.accountService = accountService;
            this.avatarStorageService = avatarStorageService;
        }

        [HttpGet("{accountId:guid}")]
        public async Task<ActionResult<AccountProfileDataTransferObject>> GetProfile(Guid accountId)
        {
            var result = await this.accountService.GetProfileAsync(accountId);
            if (!result.Success)
            {
                return NotFound(new { result.Error });
            }

            return Ok(result.Data);
        }

        [HttpPut("{accountId:guid}")]
        public async Task<IActionResult> UpdateProfile(Guid accountId, [FromBody] AccountProfileDataTransferObject body)
        {
            var result = await this.accountService.UpdateProfileAsync(accountId, body);
            if (!result.Success)
            {
                return BadRequest(new { result.Error });
            }

            return NoContent();
        }

        [HttpPut("{accountId:guid}/password")]
        public async Task<IActionResult> ChangePassword(Guid accountId, [FromBody] ChangePasswordDataTransferObject body)
        {
            var result = await this.accountService.ChangePasswordAsync(accountId, body.CurrentPassword, body.NewPassword);
            if (!result.Success)
            {
                return BadRequest(new { result.Error });
            }

            return NoContent();
        }

        [HttpPost("{accountId:guid}/avatar")]
        [RequestSizeLimit(MaximumAvatarUploadBytes)]
        public async Task<ActionResult<AvatarUploadResponseDataTransferObject>> UploadAvatar(Guid accountId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Error = "File is required." });
            }

            string extension = Path.GetExtension(file.FileName);
            string relativeUrl;
            await using (var stream = file.OpenReadStream())
            {
                relativeUrl = await this.avatarStorageService.SaveAsync(accountId, stream, extension);
            }

            var result = await this.accountService.SetAvatarUrlAsync(accountId, relativeUrl);
            if (!result.Success)
            {
                this.avatarStorageService.Delete(relativeUrl);
                return BadRequest(new { result.Error });
            }

            return Ok(new AvatarUploadResponseDataTransferObject { AvatarUrl = relativeUrl });
        }

        [HttpDelete("{accountId:guid}/avatar")]
        public async Task<IActionResult> RemoveAvatar(Guid accountId)
        {
            var result = await this.accountService.RemoveAvatarAsync(accountId);
            if (!result.Success)
            {
                return BadRequest(new { result.Error });
            }

            return NoContent();
        }
    }
}
