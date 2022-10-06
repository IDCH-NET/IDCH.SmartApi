using Microsoft.ML;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;
using System.Diagnostics;
using System.Drawing;
using IDCH.SmartApi.Helpers;
using System.Drawing.Imaging;
using IDCH.Tools;
using System.Dynamic;

namespace Models.Yolo
{
    public class YoloModel
    {
        // model is available here:
        // https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/yolov4
        static string modelPath = AppContext.BaseDirectory + @"Assets/Models/yolov4.onnx";

        //const string imageFolder = @"Assets\Images";
        AzureBlobHelper blob;
        static string imageOutputFolder = AppContext.BaseDirectory + @"Assets/Output";
        public YoloModel(AzureBlobHelper blob)
        {
            this.blob = blob;
        }
        static readonly string[] classesNames = new string[] { "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet", "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };

        static List<Color> ColorLabels = new();
        static List<Pen> PenLabels = new();
        public async Task< OutputCls> Predict(byte[] ImageData)
        {
            if (ColorLabels.Count <= 0)
            {
                var rnd = new Random(Environment.TickCount);
                for(var i = 0; i<classesNames.Length; i++)
                {
                    Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                    ColorLabels.Add(randomColor);
                    var pen = new Pen(randomColor,2);
                    PenLabels.Add(pen);
                }
            }
            var output = new OutputCls() { IsSucceed = false };
            try
            {
                if (!Directory.Exists(imageOutputFolder))
                    Directory.CreateDirectory(imageOutputFolder);
                MLContext mlContext = new MLContext();

                // model is available here:
                // https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/yolov4

                // Define scoring pipeline
                var pipeline = mlContext.Transforms.ResizeImages(inputColumnName: "bitmap", outputColumnName: "input_1:0", imageWidth: 416, imageHeight: 416, resizing: ResizingKind.IsoPad)
                    .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input_1:0", scaleImage: 1f / 255f, interleavePixelColors: true))
                    .Append(mlContext.Transforms.ApplyOnnxModel(
                        shapeDictionary: new Dictionary<string, int[]>()
                        {
                        { "input_1:0", new[] { 1, 416, 416, 3 } },
                        { "Identity:0", new[] { 1, 52, 52, 3, 85 } },
                        { "Identity_1:0", new[] { 1, 26, 26, 3, 85 } },
                        { "Identity_2:0", new[] { 1, 13, 13, 3, 85 } },
                        },
                        inputColumnNames: new[]
                        {
                        "input_1:0"
                        },
                        outputColumnNames: new[]
                        {
                        "Identity:0",
                        "Identity_1:0",
                        "Identity_2:0"
                        },
                        modelFile: modelPath, recursionLimit: 100));

                // Fit on empty list to obtain input data schema
                var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));

                // Create prediction engine
                var predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);

                // save model
                //mlContext.Model.Save(model, predictionEngine.OutputSchema, Path.ChangeExtension(modelPath, "zip"));
                var sw = new Stopwatch();
                sw.Start();
                
                    var stream = new MemoryStream(ImageData);
                using (var bitmap = new Bitmap(stream))
                {
                    // predict
                    var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
                    var results = predict.GetResults(classesNames, 0.3f, 0.7f);
                    var ListResult = new List<dynamic>();
                    //using (var g = Graphics.FromImage(bitmap))
                    {
                    
                        var captions = new List<string>();
                        var rects = new List<Rectangle>();
                        foreach (var res in results)
                        {
                            // draw predictions
                            var x1 = res.BBox[0];
                            var y1 = res.BBox[1];
                            var x2 = res.BBox[2];
                            var y2 = res.BBox[3];
                            var index = Array.FindIndex(classesNames, row => row.Contains(res.Label));
                            rects.Add(new Rectangle(Convert.ToInt32(x1), Convert.ToInt32(y1), Convert.ToInt32(x2 - x1), Convert.ToInt32(y2 - y1)));
                            captions.Add($"{res.Label}:{res.Confidence.ToString("n2")}");
                      
                            //g.DrawRectangle(PenLabels[index], x1, y1, x2 - x1, y2 - y1);
                            /*
                            using (var brushes = new SolidBrush(Color.FromArgb(50, ColorLabels[index])))
                            {
                                g.FillRectangle(brushes, x1, y1, x2 - x1, y2 - y1);
                            }

                            g.DrawString(res.Label + " " + res.Confidence.ToString("0.00"),
                                         new Font("Arial", 12), Brushes.Blue, new PointF(x1, y1));
                            */
                            dynamic hasil = new ExpandoObject();
                            hasil.confidence = res.Confidence;
                            hasil.label = res.Label;
                            hasil.pos = new float[] { x1, y1, x2 - x1, y2 - y1 };
                            ListResult.Add(hasil);
                        }
                        var img = ImageHelper.DrawBoxes(rects, captions, ImageData, "");
                        output.Data = ListResult;
                        var rand = NumberGen.GenerateNumber(5);
                        var fname = $"img_{DateTime.Now.ToString("ddMMyyyy")}_{rand}.png";
                        var resupload = await blob.UploadFile(fname, img);
                        if (resupload)
                            output.FileUrls.Add(fname);
                        output.IsSucceed = true;
                    }
                }
                
                sw.Stop();
                Console.WriteLine($"Done in {sw.ElapsedMilliseconds}ms.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                output.Message = ex.ToString();
                output.IsSucceed = false;
            }
            return output; 
        }
    
    }
}
