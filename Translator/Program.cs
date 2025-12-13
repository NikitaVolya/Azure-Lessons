using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;


namespace Translator
{
    internal class Program
    {

        static string cvKey = Environment.GetEnvironmentVariable("COMPUTER_VISION_KEY");
        static string cvEndpoint = Environment.GetEnvironmentVariable("COMPUTER_VISION_ENDPOINT");

        static private readonly List<VisualFeatureTypes?> _features = new List<VisualFeatureTypes?>()
        {
            VisualFeatureTypes.Description,
            VisualFeatureTypes.Objects,
            VisualFeatureTypes.Faces,
            VisualFeatureTypes.Brands,
            VisualFeatureTypes.Color,
            VisualFeatureTypes.Adult
        };

        private static string key = Environment.GetEnvironmentVariable("TRANSLATOR_KEY");
        private static string endpoint = Environment.GetEnvironmentVariable("TRANSLATOR_ENDPOINT");
        private static string region = "francecentral";
        private static string route = "/translate?api-version=3.0&to=ru&to=uk&to=fr&to=ja";

        public static async Task<string> ReadTextAsync(ComputerVisionClient client, string imageUrl)
        {
            var read = await client.ReadAsync(imageUrl);
            string operationId = read.OperationLocation.Split('/').Last();

            ReadOperationResult result;
            do
            {
                result = await client.GetReadResultAsync(Guid.Parse(operationId));
                await Task.Delay(300);
            }
            while (result.Status == OperationStatusCodes.Running);

            var sb = new StringBuilder();

            if (result.Status == OperationStatusCodes.Succeeded)
            {
                foreach (var page in result.AnalyzeResult.ReadResults)
                    foreach (var line in page.Lines)
                        sb.AppendLine(line.Text);
            }

            return sb.ToString();
        }

        public static async Task<ImageAnalysis> AnalyzeImageAsync(ComputerVisionClient client, string imageUrl)
                {
                    var features = new[]
                    {
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Objects,
                VisualFeatureTypes.Color
            };

            return await client.AnalyzeImageAsync(imageUrl, _features);
        }

        static async Task<Dictionary<string, string>> Translate(AzureTranslator translator, string text)
        {
            var result = await translator.TranslateTextAsync(text);

            var translations = new Dictionary<string, string>();

            foreach (var t in result[0].Translations)
                translations[t.To] = t.Text;

            return translations;
        }

        public static byte[] GenerateQrCode(string url)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrData);
            var bitmap = qrCode.GetGraphic(20);
            var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        public static async Task<byte[]> DownloadImageBytesAsync(string url)
        {
            var http = new HttpClient();
            return await http.GetByteArrayAsync(url);
        }

        public static byte[] GenerateQrCodeBytes(string url)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrData);
            var bitmap = qrCode.GetGraphic(20);
            var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        public static async Task GeneratePdfReport(
            string imageUrl, 
            string ocrText, 
            Dictionary<string, string> translations, 
            string caption, 
            Dictionary<string, string> captionTranslations, 
            string qrUrl, 
            string colorHex)
        {
            var fontPath = @"C:\Windows\Fonts\arial.ttf";
            var font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);

            var imageBytes = await DownloadImageBytesAsync(imageUrl);

            if (imageBytes == null || imageBytes.Length == 0)
                throw new Exception("Image is empty");

            byte[] qrBytes;
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrData = qrGenerator.CreateQrCode(qrUrl, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrData);
                qrBytes = qrCode.GetGraphic(20);
            }

            var writer = new PdfWriter("analysis_report.pdf");
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            document.SetFont(font);
            document.SetFontSize(12);
            document.SetMargins(20, 20, 20, 20);

            document.Add(new Paragraph("Image Analysis Report").SetFontSize(20));

            var image = new iText.Layout.Element.Image(iText.IO.Image.ImageDataFactory.Create(imageBytes));
            image.SetAutoScale(true);
            document.Add(image);

            document.Add(new Paragraph("Scene Description:"));
            document.Add(new Paragraph($"EN: {caption}"));
            document.Add(new Paragraph($"UK: {(captionTranslations.TryGetValue("uk", out var uk) ? uk : "")}"));
            document.Add(new Paragraph($"FR: {(captionTranslations.TryGetValue("fr", out var fr) ? fr : "")}"));
            document.Add(new Paragraph($"JA: {(captionTranslations.TryGetValue("ja", out var ja) ? ja : "")}"));

            document.Add(new Paragraph("Detected Text and Translations"));

            var table = new Table(UnitValue.CreatePercentArray(3)).UseAllAvailableWidth();
            table.AddHeaderCell("Original");
            table.AddHeaderCell("Language");
            table.AddHeaderCell("Translation");

            var lines = ocrText.Split('\n');
            int i = 0;
            foreach (var kvp in translations)
            {
                table.AddCell(lines.Length > i ? lines[i] : "");
                table.AddCell(kvp.Key);
                table.AddCell(kvp.Value);
                i++;
            }
            document.Add(table);

            document.Add(new Paragraph("Main Color Scheme:"));
            var color = DeviceCmyk.BLACK;
            try
            {
                System.Drawing.Color rgb = ColorTranslator.FromHtml(colorHex);
                color = DeviceCmyk.ConvertRgbToCmyk(new DeviceRgb(rgb.R, rgb.G, rgb.B));
            }
            catch { }
            var rect = new Paragraph("").SetBackgroundColor(color).SetHeight(30);
            document.Add(rect);

            document.Add(new Paragraph("QR Code:"));
            var qrImage = new iText.Layout.Element.Image(iText.IO.Image.ImageDataFactory.Create(qrBytes));
            qrImage.SetAutoScale(true);
            document.Add(qrImage);

            document.Close();
            pdf.Close();
            writer.Close();
        }

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            string imageUrl = "https://m.media-amazon.com/images/I/81cI9woulbL.jpg";
            string qrUrl = imageUrl;

            ComputerVisionClient visionClient = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(cvKey))
            {
                Endpoint = cvEndpoint
            };
            var translator = new AzureTranslator(key, endpoint, region, route);

            var analysis = await AnalyzeImageAsync(visionClient, imageUrl);
            var ocrText = await ReadTextAsync(visionClient, imageUrl);

            File.WriteAllText("recognized_text.txt", ocrText);

            Dictionary<string, string> ocrTranslations = await Translate(translator, ocrText);

            string caption = analysis.Description.Captions.FirstOrDefault()?.Text ?? "";

            var captionTranslations = await Translate(translator, caption);

            Console.WriteLine("\nTEXT TRANSLATION");
            Console.WriteLine("Original | Language | Translation");


            var jsonResult = new
            {
                image_url = imageUrl,
                detected_text = ocrText,
                translations = new
                {
                    uk = ocrTranslations.FirstOrDefault(pair => pair.Key == "uk"),
                    fr = ocrTranslations.FirstOrDefault(pair => pair.Key == "fr"),
                    zh = ocrTranslations.FirstOrDefault(pair => pair.Key == "ja")
                },
                description = new
                {
                    en = caption,
                    uk = captionTranslations.FirstOrDefault(pair => pair.Key == "uk"),
                    fr = captionTranslations.FirstOrDefault(pair => pair.Key == "fr"),
                    zh = captionTranslations.FirstOrDefault(pair => pair.Key == "ja")
                }
            };

            File.WriteAllText(
                "result.json",
                Newtonsoft.Json.JsonConvert.SerializeObject(jsonResult, Newtonsoft.Json.Formatting.Indented)
            );

            Console.WriteLine("\nJSON saved to result.json");

            string mainColor = $"#{analysis.Color.DominantColorForeground}";

            await GeneratePdfReport(
                imageUrl,
                ocrText,
                ocrTranslations,
                caption,
                captionTranslations,
                qrUrl,
                mainColor
            );

            Console.WriteLine("PDF report generated: analysis_report.pdf");
        }
    }
}
