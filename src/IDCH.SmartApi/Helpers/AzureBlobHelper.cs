using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IDCH.SmartApi.Helpers
{

    public class AzureBlobHelper
    {
        public string DocFolder { get; set; }
        public AzureBlobHelper()
        {
            try
            {
                Setup();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }

            //string imageName = $"{shortid.ShortId.Generate(false, false, 5)}_{fileName}";//+ Path.GetExtension(fileName);

        }
        async void Setup()
        {
            DocFolder = AppContext.BaseDirectory + @"Assets/Output";
            if (!Directory.Exists(DocFolder))
            {
                Directory.CreateDirectory(DocFolder);
            }
           
        }

        public async Task<byte[]> DownloadFile(string fileName)
        {
            try
            {
                if (!string.IsNullOrEmpty(DocFolder))
                {
                    var targetFile = $"{DocFolder}/{fileName}";
                    var res = await File.ReadAllBytesAsync(targetFile);
                    return res;
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

            }
            return default;
        }

        public string GetFPath(string fileName)
        {
            try
            {
                if (!string.IsNullOrEmpty(DocFolder))
                {
                    var targetFile = $"{DocFolder}/{fileName}";
                    return targetFile;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

            }
            return default;
        }
        public async Task<bool> UploadFile(string fileName, byte[] Data)
        {
            try
            {
                if (!string.IsNullOrEmpty(DocFolder))
                {
                    var targetFile = $"{DocFolder}/{fileName}";
                    File.WriteAllBytes(targetFile, Data);
                }
                //get Blob reference

                //CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                //cloudBlockBlob.Properties.ContentType = imageToUpload.ContentType;

                //await cloudBlockBlob.UploadFromByteArrayAsync(Data, 0, Data.Length);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}


