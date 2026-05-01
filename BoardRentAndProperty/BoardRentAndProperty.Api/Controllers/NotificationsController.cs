using System;
using System.Collections.Generic;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;

namespace BoardRentAndProperty.Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        [HttpGet("user/{accountId:guid}")]
        public ActionResult<IReadOnlyList<NotificationDTO>> GetForUser(Guid accountId)
        {
            return Ok(this.notificationService.GetNotificationsForUser(accountId));
        }

        [HttpGet("{notificationId:int}")]
        public ActionResult<NotificationDTO> GetById(int notificationId)
        {
            try
            {
                return Ok(this.notificationService.GetNotificationByIdentifier(notificationId));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{notificationId:int}")]
        public IActionResult Update(int notificationId, [FromBody] NotificationDTO body)
        {
            this.notificationService.UpdateNotificationByIdentifier(notificationId, body);
            return NoContent();
        }

        [HttpDelete("{notificationId:int}")]
        public ActionResult<NotificationDTO> Delete(int notificationId)
        {
            try
            {
                return Ok(this.notificationService.DeleteNotificationByIdentifier(notificationId));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
