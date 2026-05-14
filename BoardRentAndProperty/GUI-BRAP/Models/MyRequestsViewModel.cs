using System.Collections.Generic;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace GUI_BRAP.Models
{
    public class MyRequestsViewModel
    {
        public IReadOnlyList<RequestDTO> Requests { get; init; } = new List<RequestDTO>();

        public string? ErrorMessage { get; init; }
    }
}
