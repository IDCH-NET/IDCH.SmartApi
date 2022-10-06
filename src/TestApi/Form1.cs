using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows.Forms;

namespace TestApi
{
    public partial class Form1 : Form
    {
        string FileSelect = string.Empty;
        //string BaseUrl = "https://localhost:7110";
        string BaseUrl = "https://smartapi.my.id";
        public Form1()
        {
            InitializeComponent();
            BtnPick.Click += (a, b) => { // open file dialog   
                OpenFileDialog open = new OpenFileDialog();
                // image filters  
                open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
                if (open.ShowDialog() == DialogResult.OK)
                {
                    // display image in picture box  
                    Pic1.Image = new Bitmap(open.FileName);
                    // image file path  
                    FileSelect = open.FileName;
                }
            };
            BtnClear.Click += (a, b) => {
                Pic2.Image = null;
                TxtInfo.Clear();
            };
            BtnDetect.Click += async(a, b) => {

                if (string.IsNullOrEmpty(FileSelect))
                {
                    MessageBox.Show("Please select image first.");
                    return;
                }
                try
                {
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromMinutes(2);

                    using var formData = new MultipartFormDataContent();
                    await using var file = File.OpenRead(FileSelect);
                    var streamContent = new StreamContent(file);
                    formData.Add(streamContent, "file", Path.GetFileName(FileSelect));
                    //content.Add(fileContent, "file", Path.GetFileName(FileSelect));
                    /*
                    var content = new MultipartFormDataContent();
                    var file = new MemoryStream(File.ReadAllBytes(FileSelect));
                    var fileContent = new StreamContent(file);
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse($"image/{Path.GetExtension(FileSelect).Replace(".","")}");
                    var filenameOnly = Path.GetFileNameWithoutExtension(FileSelect);
                    var fileName = Path.GetFileName(FileSelect);
                    fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                    {
                        Name = $"files[{filenameOnly}]",
                        FileName = fileName
                    };
                    content.Add(fileContent, $"files[{filenameOnly}]");
                    */
                    var response = await client.PostAsync($"{BaseUrl}/api/Model/DetectObject",
                        formData);
                    TxtInfo.Clear();
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var setting = new JsonSerializerSettings() { Formatting = Formatting.Indented };
                        var output = JsonConvert.DeserializeObject<OutputCls>(data,setting);
                        TxtInfo.Text = data;
                        foreach(var filestr in output.FileUrls)
                        {
                            var fileImg = await client.GetByteArrayAsync(filestr);
                            if (fileImg != null)
                            {
                                var mem = new MemoryStream(fileImg);
                                var bitmap = new Bitmap(mem);
                                Pic2.Image = bitmap;
                            }
                        }
                    }
                    else
                    {

                        TxtInfo.Text = "fail to call api, check your internet";
                    }


                }
                catch (Exception ex)
                {
                    TxtInfo.Text = "error:"+ ex.ToString();
                }
             

            };
        }
    }
    public class OutputCls
    {
        public bool IsSucceed { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public List<string> FileUrls { get; set; } = new();
    }
}