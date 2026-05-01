using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;

namespace BoardRentAndProperty.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 100;

        private readonly IAdminService adminService;

        public AdminController(IAdminService adminService)
        {
            this.adminService = adminService;
        }

        [HttpGet("accounts")]
        public async Task<ActionResult<List<AccountProfileDataTransferObject>>> GetAccounts(
            [FromQuery] int page = DefaultPageNumber,
            [FromQuery] int pageSize = DefaultPageSize)
        {
            var result = await this.adminService.GetAllAccountsAsync(page, pageSize);
            if (!result.Success)
            {
                return BadRequest(new { result.Error });
            }

            return Ok(result.Data);
        }

        [HttpPut("accounts/{accountId:guid}/suspend")]
        public async Task<IActionResult> Suspend(Guid accountId)
        {
            var result = await this.adminService.SuspendAccountAsync(accountId);
            if (!result.Success)
            {
                return BadRequest(new { result.Error });
            }

            return NoContent();
        }

        [HttpPut("accounts/{accountId:guid}/unsuspend")]
        public async Task<IActionResult> Unsuspend(Guid accountId)
        {
            var result = await this.adminService.UnsuspendAccountAsync(accountId);
            if (!result.Success)
            {
                return BadRequest(new { result.Error });
            }

            return NoContent();
        }

        [HttpPut("accounts/{accountId:guid}/reset-password")]
        public async Task<IActionResult> ResetPassword(Guid accountId, [FromBody] ResetPasswordDataTransferObject body)
        {
            var result = await this.adminService.ResetPasswordAsync(accountId, body.NewPassword);
            if (!result.Success)
            {
                return BadRequest(new { result.Error });
            }

            return NoContent();
        }

        [HttpPut("accounts/{accountId:guid}/unlock")]
        public async Task<IActionResult> Unlock(Guid accountId)
        {
            var result = await this.adminService.UnlockAccountAsync(accountId);
            if (!result.Success)
            {
                return BadRequest(new { result.Error });
            }

            return NoContent();
        }
    }
}
