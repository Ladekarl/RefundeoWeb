using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Models;

namespace Refundeo.Controllers
{
    [Route("/api/tag")]
    public class TagController : Controller
    {
        private readonly RefundeoDbContext _context;

        public TagController(RefundeoDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<IList<TagDto>> GetAllTags()
        {
            return await _context.Tags.Select(t => new TagDto
            {
                Key = t.Key,
                Value = t.Value
            }).ToListAsync();
        }
    }
}
