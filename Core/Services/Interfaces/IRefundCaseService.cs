using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Models.RefundCase;

namespace Refundeo.Core.Services.Interfaces
{
    public interface IRefundCaseService
    {
        Task<ObjectResult> GenerateRefundCaseDtoResponseAsync(IEnumerable<RefundCase> refundCases);
        Task<ObjectResult> GenerateRefundCaseDtoResponseAsync(RefundCase refundCase);
        Task<RefundCaseDto> ConvertRefundCaseToDtoAsync(RefundCase refundCase);
        RefundCaseSimpleDto ConvertRefundCaseToDtoSimple(RefundCase refundCase);
        Task DeleteRefundCasesAsync(ICollection<RefundCase> refundCases);
    }
}
