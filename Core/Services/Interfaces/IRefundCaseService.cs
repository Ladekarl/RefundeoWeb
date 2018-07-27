using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Models.RefundCase;

namespace Refundeo.Core.Services.Interfaces
{
    public interface IRefundCaseService
    {
        Task<ObjectResult> GenerateRefundCaseDtoResponseAsync(IEnumerable<RefundCase> refundCases,
            RefundeoUser callingUser);

        Task<ObjectResult> GenerateRefundCaseDtoResponseAsync(RefundCase refundCase,
            RefundeoUser callingUser);

        Task<RefundCaseDto> ConvertRefundCaseToDtoAsync(RefundCase refundCase, RefundeoUser callingUser);
        RefundCaseSimpleDto ConvertRefundCaseToDtoSimple(RefundCase refundCase);
        Task DeleteRefundCasesAsync(ICollection<RefundCase> refundCases);
    }
}
