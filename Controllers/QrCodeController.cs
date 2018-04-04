using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Refundeo.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Refundeo.Data;
using Refundeo.Data.Models;

namespace Refundeo.Controllers
{
    [Authorize(Roles = "Admin,Merchant")]
    [Route("api/[controller]")]
    public class QRCodeController : Controller
    {
        private readonly RefundeoDbContext _context;
        public QRCodeController(RefundeoDbContext context) 
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IList<QRCode>> GetAll() 
        {
            return await _context.QRCodes.ToListAsync();
        }

        [HttpGet("{id}", Name = "GetQRCode")]
        public async Task<IActionResult> GetById(long id)
        {
            var qrCode = await _context.QRCodes.FirstOrDefaultAsync(q => q.Id == id);
            if (qrCode == null)
            {
                return NotFound();
            }
            return new ObjectResult(qrCode);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QRCodeDTO qrCode)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _context.QRCodes.AddAsync(new QRCode {
                Name = qrCode.Name,
                IsComplete = qrCode.IsComplete
            });

            await _context.SaveChangesAsync();

            return CreatedAtRoute("GetQRCode", new { id = result.Entity.Id}, result.Entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] QRCode qrCode)
        {
            if (qrCode == null || qrCode.Id != id)
            {
                return BadRequest();
            }

            var qrCodeToUpdate = await _context.QRCodes.FirstOrDefaultAsync(t => t.Id == id);
            if (qrCodeToUpdate == null)
            {
                return NotFound();
            }

            qrCodeToUpdate.IsComplete = qrCode.IsComplete;
            qrCodeToUpdate.Name = qrCode.Name;

            _context.QRCodes.Update(qrCodeToUpdate);
            await _context.SaveChangesAsync();
            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var todo = await _context.QRCodes.FirstOrDefaultAsync(q => q.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            _context.QRCodes.Remove(todo);
            await _context.SaveChangesAsync();
            return new NoContentResult();
        }
    }
}