using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Refundeo.Core.Data.Models;
using Refundeo.Core.Models.QRCode;
using Refundeo.Core.Models.RefundCase;
using Refundeo.Core.Services.Interfaces;
using ZXing;
using ZXing.QrCode;

namespace Refundeo.Core.Services
{
    public class RefundCaseService : IRefundCaseService
    {
        private readonly IUtilityService _utilityService;

        public RefundCaseService(IUtilityService utilityService)
        {
            _utilityService = utilityService;
        }

        public ObjectResult GenerateRefundCaseDtoResponse(IEnumerable<RefundCase> refundCases)
        {
            var dtos = new List<RefundCaseDto>();
            foreach (var refundCase in refundCases)
            {
                dtos.Add(ConvertRefundCaseToDto(refundCase));
            }

            return new ObjectResult(dtos);
        }

        public ObjectResult GenerateRefundCaseDtoResponse(RefundCase refundCase)
        {
            return new ObjectResult(ConvertRefundCaseToDto(refundCase));
        }

        public RefundCaseDto ConvertRefundCaseToDto(RefundCase refundCase)
        {
            return new RefundCaseDto
            {
                Id = refundCase.Id,
                Amount = refundCase.Amount,
                RefundAmount = refundCase.RefundAmount,
                IsRequested = refundCase.IsRequested,
                IsAccepted = refundCase.IsAccepted,
                IsRejected = refundCase.IsRejected,
                QrCode = ConvertByteArrayToBase64(refundCase.QRCode?.Image),
                Documentation = ConvertByteArrayToBase64(refundCase.Documentation?.Image),
                DateCreated = refundCase.DateCreated,
                DateRequested = refundCase.DateRequested,
                Customer = _utilityService.ConvertCustomerInformationToDto(refundCase.CustomerInformation),
                Merchant = _utilityService.ConvertMerchantInformationToDto(refundCase.MerchantInformation),
            };
        }

        public byte[] GenerateQrCode(int height, int width, int margin, QRCodePayloadDto payload)
        {
            var qrCodeWriter = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width,
                    Margin = margin
                }
            };
            var pixelData = qrCodeWriter.Write(JsonConvert.SerializeObject(payload));
            byte[] image;
            using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height,
                System.Drawing.Imaging.PixelFormat.Format32bppRgb))
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

        public string ConvertByteArrayToBase64(byte[] ba)
        {
            return ba != null ? Convert.ToBase64String(ba) : null;
        }

        public byte[] ConvertBase64ToByteArray(string base64String)
        {
            byte[] ba = null;
            if (base64String != null)
            {
                ba = Convert.FromBase64String(base64String);
            }

            return ba;
        }
    }
}
