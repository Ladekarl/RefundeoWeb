using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Refundeo.Core.Data;
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
        private IUtilityService _utilityService;
        public RefundCaseService(IUtilityService utilityService)
        {
            _utilityService = utilityService;
        }

        public ObjectResult GenerateRefundCaseDTOResponse(IEnumerable<RefundCase> refundCases)
        {
            var dtos = new List<RefundCaseDTO>();
            foreach (var refundCase in refundCases)
            {
                dtos.Add(ConvertRefundCaseToDTO(refundCase));
            }
            return new ObjectResult(dtos);
        }

        public ObjectResult GenerateRefundCaseDTOResponse(RefundCase refundCase)
        {
            return new ObjectResult(ConvertRefundCaseToDTO(refundCase));
        }

        public RefundCaseDTO ConvertRefundCaseToDTO(RefundCase refundCase)
        {
            return new RefundCaseDTO
            {
                Id = refundCase.Id,
                Amount = refundCase.Amount,
                RefundAmount = refundCase.RefundAmount,
                IsRequested = refundCase.IsRequested,
                IsAccepted = refundCase.IsAccepted,
                QRCode = ConvertByteArrayToBase64(refundCase?.QRCode?.Image),
                Documentation = ConvertByteArrayToBase64(refundCase?.Documentation?.Image),
                DateCreated = refundCase.DateCreated,
                DateRequested = refundCase.DateRequested,
                Customer = _utilityService.ConvertCustomerInformationToDTO(refundCase.CustomerInformation),
                Merchant = _utilityService.ConvertMerchantInformationToDTO(refundCase.MerchantInformation),
            };
        }

        public byte[] GenerateQRCode(int height, int width, int margin, QRCodePayloadDTO payload)
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

        public string ConvertByteArrayToBase64(byte[] ba)
        {
            string base64String = null;
            if (ba != null)
            {
                return Convert.ToBase64String(ba);
            }
            return base64String;
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