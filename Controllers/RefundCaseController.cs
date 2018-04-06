using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Refundeo.Data;
using Refundeo.Data.Models;
using Refundeo.Models.QRCode;
using Refundeo.Models.RefundCase;
using ZXing;
using ZXing.QrCode;

namespace Refundeo.Controllers
{
    public class RefundCaseController : RefundeoController
    {
        public RefundCaseController(RefundeoDbContext context, UserManager<RefundeoUser> userManager) : base(context, userManager)
        {
        }

        protected async Task<ObjectResult> GenerateRefundCaseDTOResponseAsync(IEnumerable<RefundCase> refundCases)
        {
            var dtos = new List<RefundCaseDTO>();
            foreach (var refundCase in refundCases)
            {
                dtos.Add(await ConvertRefundCaseToDTOAsync(refundCase));
            }
            return new ObjectResult(dtos);
        }

        protected async Task<ObjectResult> GenerateRefundCaseDTOResponseAsync(RefundCase refundCase)
        {
            return new ObjectResult(await ConvertRefundCaseToDTOAsync(refundCase));
        }

        protected async Task<RefundCaseDTO> ConvertRefundCaseToDTOAsync(RefundCase refundCase)
        {
            return new RefundCaseDTO
            {
                Id = refundCase.Id,
                Amount = refundCase.Amount,
                RefundAmount = refundCase.RefundAmount,
                IsRequested = refundCase.IsRequested,
                QRCode = ConvertByteArrayToBase64(refundCase?.QRCode?.Image),
                Documentation = ConvertByteArrayToBase64(refundCase?.Documentation?.Image),
                Customer = await ConvertRefundeoUserToUserDTOAsync(refundCase.Customer),
                Merchant = await ConvertRefundeoUserToUserDTOAsync(refundCase.Merchant)
            };
        }

        protected byte[] GenerateQRCode(int height, int width, int margin, QRCodePayloadDTO payload)
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
            byte[] image = null;
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
                image = ms.ToArray();
            }
            return image;
        }

        protected string ConvertByteArrayToBase64(byte[] ba)
        {
            string base64String = null;
            if (ba != null)
            {
                return Convert.ToBase64String(ba);
            }
            return base64String;
        }

        protected byte[] ConvertBase64ToByteArray(string base64String)
        {
            byte[] ba = null;
            if (ba != null)
            {
                ba = Convert.FromBase64String(base64String);
            }
            return ba;
        }
    }
}