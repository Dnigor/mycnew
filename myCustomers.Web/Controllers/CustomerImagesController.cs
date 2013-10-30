using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Web.Mvc;
using System.Web.UI;
using MaryKay.Configuration;
using myCustomers.Contexts;
using Quartet.Entities;
using Quartet.Entities.Commands;

namespace myCustomers.Web.Controllers
{
    [AuthorizeConsultant]
    public class CustomerImagesController : Controller
    {
        readonly IQuartetClientFactory _clientFactory;
        readonly IConsultantContext _consultantContext;
        readonly IAppSettings _appSettings;

        public CustomerImagesController(IQuartetClientFactory clientFactory, IConsultantContext consultantContext, IAppSettings appSettings)
        {
            _clientFactory     = clientFactory;
            _consultantContext = consultantContext;
            _appSettings       = appSettings;
        }

        // TODO: support http post for uploading an image

        [HttpGet]
        [ActionName("profile")]
        [OutputCache(VaryByParam = "*", Duration = int.MaxValue, Location = OutputCacheLocation.Downstream)]
        public ActionResult GetProfilePhoto
        (
            Guid customerId,
            [Bind(Prefix = "ts")] string lastUpdatedDateUtc, // Not sure if this is needed for the output cache
            [Bind(Prefix = "h")] int height = 200
        )
        {
            if (!Request.IsAuthenticated)
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

            var client = _clientFactory.GetCustomerPictureServiceClient();

            var consultant    = _consultantContext.Consultant;
            var consultantKey = consultant.ConsultantKey.Value;
            var subsidiary    = consultant.SubsidiaryCode;

            var imageData = client.GetPicture(customerId, PictureType.ListViewThumbnail.ToString());
            if (imageData == null)
                return HttpNotFound();

            return new ImageResult(imageData, height);
        }

        [HttpPost]
        [ActionName("profile")]
        public void PostProfilePhoto(Guid id)
        {
            var files = Request.Files;

            if (files == null || files.Count != 1 || files[0].ContentLength == 0)
                return;

            var postedFile     = files[0];
            var fileSizeLimit  = _appSettings.GetValue<int>("ProfileImage.MaxFileSizeInMB") * 1024 * 1024;
            var maxImageWidth  = _appSettings.GetValue<int>("ProfileImage.MaxWidth");
            var maxImageHeight = _appSettings.GetValue<int>("ProfileImage.MaxHeight");

            if (postedFile.ContentLength > fileSizeLimit)
                throw new InvalidOperationException("Invalid file size");

            var bytes = new byte[postedFile.ContentLength];
            postedFile.InputStream.Read(bytes, 0, postedFile.ContentLength);

            using (var inputStream = new MemoryStream(bytes))
            using (var image = Image.FromStream(inputStream))
            using (var paddedImage = ScaleImage(image, maxImageWidth, maxImageHeight))
            using (var outputStream = new MemoryStream())
            {
                paddedImage.Save(outputStream, ImageFormat.Png);
                outputStream.Flush();

                var pictureServiceClient = _clientFactory.GetCustomerPictureServiceClient();
                pictureServiceClient.SavePicture(outputStream.ToArray(), null, id, PictureType.ListViewThumbnail.ToString());
            }
        }

        [HttpDelete]
        [ActionName("profile")]
        public void DeleteProfilePhoto(Guid id)
        {
            var client = _clientFactory.GetCommandServiceClient();
            client.Execute(new DeletePicture
            {
                CustomerId = id,
                PictureType = PictureType.ListViewThumbnail
            });
        }

        // Passing null for either maxWidth or maxHeight maintains aspect ratio while
        //        the other non-null parameter is guaranteed to be constrained to
        //        its maximum value.
        //
        //  Example: maxHeight = 50, maxWidth = null
        //    Constrain the height to a maximum value of 50, respecting the aspect
        //    ratio, to any width.
        //
        //  Example: maxHeight = 100, maxWidth = 90
        //    Constrain the height to a maximum of 100 and width to a maximum of 90
        //    whichever comes first.
        //
        static Size ScaleSize(Size from, int? maxWidth, int? maxHeight)
        {
            if (!maxWidth.HasValue && !maxHeight.HasValue) throw new ArgumentException("At least one scale factor (toWidth or toHeight) must not be null.");
            if (from.Height == 0 || from.Width == 0) throw new ArgumentException("Cannot scale size from zero.");

            double? widthScale = null;
            double? heightScale = null;

            if (maxWidth.HasValue)
            {
                widthScale = maxWidth.Value / (double)from.Width;
            }
            if (maxHeight.HasValue)
            {
                heightScale = maxHeight.Value / (double)from.Height;
            }

            double scale = Math.Min((double)(widthScale ?? heightScale),
                                     (double)(heightScale ?? widthScale));

            return new Size((int)Math.Floor(from.Width * scale), (int)Math.Ceiling(from.Height * scale));
        }

        static Image ScaleImage(Image originalImage, int maxWidth, int maxHeight)
        {
            if (originalImage.Width < maxWidth && originalImage.Height < maxHeight)
            {
                // scale to fit aspect ration
                var w = originalImage.Width;
                var h = originalImage.Height;

                if (w > h)
                    h = Convert.ToInt32((double)maxHeight * (double)w / (double)maxWidth);
                else
                    w = Convert.ToInt32((double)maxWidth * (double)h / (double)maxHeight);

                maxWidth = w;
                maxHeight = h;
            }

            var scaledImage = new Bitmap(maxWidth, maxHeight, PixelFormat.Format32bppArgb);
            var scale = ScaleSize(originalImage.Size, maxWidth, null);
            if (scale.Height > maxHeight)
                scale = ScaleSize(originalImage.Size, null, maxHeight);

            using (var graphics = Graphics.FromImage(scaledImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode  = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode      = SmoothingMode.HighQuality;

                graphics.Clear(Color.Transparent);
                graphics.DrawImage(originalImage, (scaledImage.Width - scale.Width) / 2, (scaledImage.Height - scale.Height) / 2, scale.Width, scale.Height);
                graphics.Flush();
            }

            return scaledImage;
        }

        public class ImageResult : ActionResult
        {
            byte[] _imageData;
            int _height;

            public ImageResult(byte[] imageData, int height)
            {
                _imageData = imageData;
                _height    = height;
            }

            public override void ExecuteResult(ControllerContext context)
            {
                context.HttpContext.Response.ContentType = "image/png";

                using (var stream = new MemoryStream(_imageData))
                using (var image = Image.FromStream(stream, true))
                {
                    var height = image.Height;
                    var scale = (double)_height / (double)height;

                    using (var bitmap = new Bitmap((int)(image.Width * scale), (int)(image.Height * scale), PixelFormat.Format32bppArgb))
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode  = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode      = SmoothingMode.HighQuality;

                        graphics.Clear(Color.Transparent);
                        graphics.DrawImage(image, 0, 0, (int)(image.Width * scale), (int)(image.Height * scale));
                        graphics.Flush();

                        bitmap.Save(context.HttpContext.Response.OutputStream, ImageFormat.Png);
                    }
                }
            }
        }
    }
}
