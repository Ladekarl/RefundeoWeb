using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Refundeo.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class QrCodeController : Controller
    {
        private readonly RefundeoDbContext _context;
        public QrCodeController(RefundeoDbContext context) 
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IList<QrCode>> GetAll() 
        {
            return await _context.QrCodes.ToListAsync();
        }

        [HttpGet("{id}", Name = "GetQrCode")]
        public async Task<IActionResult> GetById(long id)
        {
            var qrCode = await _context.QrCodes.FirstOrDefaultAsync(q => q.Id == id);
            if(qrCode == null) 
            {
                return NotFound();
            }
            return new ObjectResult(qrCode);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QrCode qrCode) 
        {
            if(qrCode == null) 
            {
                return BadRequest();
            }

            await _context.QrCodes.AddAsync(qrCode);
            await _context.SaveChangesAsync();

            return CreatedAtRoute("GetQrCode", new { id = qrCode.Id}, qrCode);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] QrCode qrCode)
        {
            if (qrCode == null || qrCode.Id != id)
            {
                return BadRequest();
            }

            var qrCodeToUpdate = await _context.QrCodes.FirstOrDefaultAsync(t => t.Id == id);
            if (qrCodeToUpdate == null)
            {
                return NotFound();
            }

            qrCodeToUpdate.IsComplete = qrCode.IsComplete;
            qrCodeToUpdate.Name = qrCode.Name;

            _context.QrCodes.Update(qrCodeToUpdate);
            await _context.SaveChangesAsync();
            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var todo = await _context.QrCodes.FirstOrDefaultAsync(q => q.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            _context.QrCodes.Remove(todo);
            await _context.SaveChangesAsync();
            return new NoContentResult();
        }
    }
}