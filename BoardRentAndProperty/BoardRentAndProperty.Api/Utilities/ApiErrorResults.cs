using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoardRentAndProperty.Api.Utilities
{
    public static class ApiErrorResults
    {
        public static ActionResult FromServiceError(this ControllerBase controller, string? error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                return controller.ApiBadRequest("Request failed.");
            }

            if (Contains(error, "not found"))
            {
                return controller.ApiNotFound(error, "not_found");
            }

            if (Contains(error, "invalid username or password") || Contains(error, "current password is incorrect"))
            {
                return controller.ApiUnauthorized(error, "invalid_credentials");
            }

            if (Contains(error, "suspended"))
            {
                return controller.ApiForbidden(error, "account_suspended");
            }

            if (Contains(error, "unauthorized access"))
            {
                return controller.ApiForbidden(error, "forbidden");
            }

            if (Contains(error, "already taken") || Contains(error, "already exists"))
            {
                return controller.ApiConflict(error, "resource_conflict");
            }

            if (error.Contains('|') || error.Contains(';') || Contains(error, "must be") || Contains(error, "invalid"))
            {
                return controller.ApiValidation(error);
            }

            return controller.ApiBadRequest(error);
        }

        public static ActionResult ApiValidation(this ControllerBase controller, string error, string code = "validation_failed") =>
            Create(StatusCodes.Status400BadRequest, code, error);

        public static ActionResult ApiBadRequest(this ControllerBase controller, string error, string code = "request_failed") =>
            Create(StatusCodes.Status400BadRequest, code, error);

        public static ActionResult ApiUnauthorized(this ControllerBase controller, string error, string code = "unauthorized") =>
            Create(StatusCodes.Status401Unauthorized, code, error);

        public static ActionResult ApiForbidden(this ControllerBase controller, string error, string code = "forbidden") =>
            Create(StatusCodes.Status403Forbidden, code, error);

        public static ActionResult ApiNotFound(this ControllerBase controller, string error, string code = "not_found") =>
            Create(StatusCodes.Status404NotFound, code, error);

        public static ActionResult ApiConflict(this ControllerBase controller, string error, string code = "conflict") =>
            Create(StatusCodes.Status409Conflict, code, error);

        private static ActionResult Create(int status, string code, string error) =>
            new ObjectResult(new ApiErrorResponse
            {
                Code = code,
                Error = error,
                Status = status,
            })
            {
                StatusCode = status,
            };

        private static bool Contains(string source, string value) =>
            source.Contains(value, StringComparison.OrdinalIgnoreCase);
    }
}
