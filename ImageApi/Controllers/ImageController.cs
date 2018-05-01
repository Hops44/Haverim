using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace ImageApi.Controllers
{
    public class ImageController : ApiController
    {
        private string GetFolderPath(string folderName, string name, string fileExtension)
        {
            return AppDomain.CurrentDomain.GetData("DataDirectory").ToString() + "\\" + folderName + "\\" + name + "." + fileExtension;
        }

        public HttpResponseMessage GetImage(string name)
        {
            Image img = Image.FromFile(GetFolderPath("Images", name, "png"));
            using (MemoryStream MemoryImageSteram = new MemoryStream())
            {
                img.Save(MemoryImageSteram, System.Drawing.Imaging.ImageFormat.Png);
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new ByteArrayContent(MemoryImageSteram.ToArray());
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                return result;
            }
        }

        public string PostUploadImage([FromBody]string Base64Data)
        {
            var DataSplit = Base64Data.Split(',');
            string Metadata;
            string ImageData;
            if (DataSplit.Length > 1)
            {
                Metadata = DataSplit[0];
                ImageData = DataSplit[1];
            }
            else
                ImageData = DataSplit[0];

            var bytes = Convert.FromBase64String(ImageData);
            var ImageId = Guid.NewGuid().ToString();
            using (var ImageMemmoryStream = new MemoryStream(bytes, 0, bytes.Length))
            {
                Image image = Image.FromStream(ImageMemmoryStream, true);
                image.Save(GetFolderPath("Images", ImageId, "png"));
            }

            return ImageId;
        }
    }
}
