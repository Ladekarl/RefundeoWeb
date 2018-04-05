using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Refundeo.Data;
using Refundeo.Data.Models;
using ZXing;
using ZXing.QrCode;
using Refundeo.Models.QRCode;
using System.IO;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace Refundeo.Controllers.Merchant
{
    [Authorize(Roles = "Merchant")]
    [Route("api/merchant/qrcode")]
    public class MerchantQRCodeController : RefundeoController
    {
        private readonly UserManager<RefundeoUser> _userManager;
        private readonly RefundeoDbContext _context;
        public MerchantQRCodeController(RefundeoDbContext context, UserManager<RefundeoUser> userManager) : base(userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMerchantQRCodes()
        {
            var user = await GetCallingUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var qrCodes = await _context.QRCodes
                .Where(q => q.Merchant == user)
                .Select(q => new QRCodeDTO
                {
                    Id = q.Id,
                    Amount = q.Amount,
                    RefundAmount = q.RefundAmount,
                    Image = q.Image
                }).ToListAsync();

            return new ObjectResult(qrCodes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMerchantQRCodeById(long id)
        {
            var user = await GetCallingUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var qrCode = await _context.QRCodes.FirstOrDefaultAsync(q => q.Id == id);

            if (qrCode == null)
            {
                return NotFound();
            }

            if (user.Id != qrCode.Merchant.Id)
            {
                return Unauthorized();
            }

            return new ObjectResult(new QRCodeDTO
            {
                Id = qrCode.Id,
                Amount = qrCode.Amount,
                RefundAmount = qrCode.RefundAmount,
                Image = qrCode.Image
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateQRCode([FromBody] CreateQRCodeDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var user = await GetCallingUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var payload = new QRCodePayloadDTO
            {
                MerchantId = user.Id,
                // TODO: RefundAmount should be calculated based on merchant
                RefundAmount = model.Amount
            };

            var base64str = GenerateQRCode(model.Height, model.Width, model.Margin, payload);

            var result = await _context.QRCodes.AddAsync(new QRCode
            {
                Amount = model.Amount,
                RefundAmount = model.Amount,
                Image = base64str,
                Merchant = user
            });

            await _context.SaveChangesAsync();

            return new ObjectResult(new QRCodeDTO
            {
                Id = result.Entity.Id,
                Amount = result.Entity.Amount,
                RefundAmount = result.Entity.RefundAmount,
                Image = result.Entity.Image
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var user = await GetCallingUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var qrCode = await _context.QRCodes.FirstOrDefaultAsync(q => q.Id == id);
            if (qrCode == null)
            {
                return NotFound();
            }

            if (qrCode.Merchant != user)
            {
                return Unauthorized();
            }

            _context.QRCodes.Remove(qrCode);
            await _context.SaveChangesAsync();
            return new NoContentResult();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] QRCodeDTO qrCode)
        {
            if (qrCode == null)
            {
                return BadRequest();
            }

            var qrCodeToUpdate = await _context.QRCodes.FirstOrDefaultAsync(t => t.Id == id);
            if (qrCodeToUpdate == null)
            {
                return NotFound();
            }

            // qrCodeToUpdate.IsComplete = qrCode.IsComplete;
            // qrCodeToUpdate.Name = qrCode.Name;

            _context.QRCodes.Update(qrCodeToUpdate);
            await _context.SaveChangesAsync();
            return new NoContentResult();
        }

        private string GenerateQRCode(int height, int width, int margin, QRCodePayloadDTO payload)
        {
            var qrCodeWriter = new BarcodeWriterPixelData
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width,
                    Margin = margin
                }
            };
            var pixelData = qrCodeWriter.Write(JsonConvert.SerializeObject(payload));
            string base64str = null;
            using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            using (var ms = new MemoryStream())
            {
                var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0,
                    pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                base64str = $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}";
            }
            return base64str;
        }
    }
}