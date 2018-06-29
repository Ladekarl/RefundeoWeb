using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Refundeo.Core.Data;
using Refundeo.Core.Data.Models;

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
        public async Task<IList<Tag>> GetAllTags()
        {
            return await _context.Tags.ToListAsync();
        }
    }
}
