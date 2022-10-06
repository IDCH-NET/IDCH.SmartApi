using Newtonsoft.Json;
using System.Net;

namespace TestApi
{
    public partial class Form1 : Form
    {
        string FileSelect = string.Empty;
        string BaseUrl = "https://localhost:7110";
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
            BtnDetect.Click += async(a, b) => {

                if (string.IsNullOrEmpty(FileSelect))
                {
                    MessageBox.Show("Please select image first.");
                    return;
                }
                try
                {
                    using var client = new HttpClient();

                    using var formData = new MultipartFormDataContent();
                    await using var file = File.OpenRead(FileSelect);
                    var streamContent = new StreamContent(file);
                    formData.Add(streamContent, "file", Path.GetFileName(FileSelect));

                    var response = await client.PostAsync($"{BaseUrl}/detectobject",
                        formData);
                    TxtInfo.Clear();
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var output = JsonConvert.DeserializeObject<OutputCls>(data);
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