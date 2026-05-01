using System;
using System.Collections.Generic;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;

namespace BoardRentAndProperty.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService userService;

        public UsersController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet("except/{excludeAccountId:guid}")]
        public ActionResult<IReadOnlyList<UserDTO>> GetUsersExcept(Guid excludeAccountId)
        {
            return Ok(this.userService.GetUsersExcept(excludeAccountId));
        }
    }
}
