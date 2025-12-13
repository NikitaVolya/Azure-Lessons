using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;



namespace ComputerVision
{
    internal class Exercice2
    {
        private ComputerVisionClient _client;
        private List<VisualFeatureTypes?> _features = new List<VisualFeatureTypes?>()
        {
            VisualFeatureTypes.Description,
            VisualFeatureTypes.Objects,
            VisualFeatureTypes.Faces,
            VisualFeatureTypes.Brands,
            VisualFeatureTypes.Color,
            VisualFeatureTypes.Adult
        };

        public Exercice2(ComputerVisionClient client)
        {
            _client = client;
        }

        public async Task<string> TextRecognition(string url)
        {
            var read = await _client.ReadAsync(url);
            string operationId = read.OperationLocation.Split('/').Last();

            ReadOperationResult readResult;
            do
            {
                readResult = await _client.GetReadResultAsync(Guid.Parse(operationId));
                await Task.Delay(300);
            }
            while (readResult.Status == OperationStatusCodes.Running);

            var extractedText = new StringBuilder();

            if (readResult.Status == OperationStatusCodes.Succeeded)
            {
                foreach (var page in readResult.AnalyzeResult.ReadResults)
                {
                    foreach (var line in page.Lines)
                    {
                        extractedText.AppendLine(line.Text);
                    }
                }
            }

            File.WriteAllText("recognized_text.txt", extractedText.ToString());

            return extractedText.ToString();
        }

        public async Task GenerateReport(ImageAnalysis analysis, string imageUrl)
        {
            var writer = new PdfWriter("analysis_report.pdf");
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);

            doc.Add(new Paragraph("Image Analysis Report"));
            doc.Add(new Paragraph($"Source: {imageUrl}\n"));

            doc.Add(new Paragraph("Scene Description"));
            foreach (var c in analysis.Description.Captions)
                doc.Add(new Paragraph($"{c.Text} ({c.Confidence})"));

            var table = new Table(4);
            table.AddHeaderCell("Object");
            table.AddHeaderCell("Probability");
            table.AddHeaderCell("Coordinates");
            table.AddHeaderCell("Type");

            foreach (var o in analysis.Objects)
            {
                table.AddCell(o.ObjectProperty);
                table.AddCell(o.Confidence.ToString());
                table.AddCell($"X:{o.Rectangle.X},Y:{o.Rectangle.Y}");
                table.AddCell("Object");
            }

            foreach (var b in analysis.Brands)
            {
                table.AddCell(b.Name);
                table.AddCell(b.Confidence.ToString());
                table.AddCell($"X:{b.Rectangle.X},Y:{b.Rectangle.Y}");
                table.AddCell("Brand");
            }

            foreach (var f in analysis.Faces)
            {
                table.AddCell($"Face ({f.Gender}, {f.Age})");
                table.AddCell("1.0");
                table.AddCell($"X:{f.FaceRectangle.Left},Y:{f.FaceRectangle.Top}");
                table.AddCell("Face");
            }

            doc.Add(table);

            doc.Add(new Paragraph("\nColor Palette"));
            doc.Add(new Paragraph($"Foreground: {analysis.Color.DominantColorForeground}"));
            doc.Add(new Paragraph($"Background: {analysis.Color.DominantColorBackground}"));
            doc.Add(new Paragraph($"Accent: {analysis.Color.AccentColor}"));

            string extractedText = await TextRecognition(imageUrl);
            if (!string.IsNullOrWhiteSpace(extractedText.ToString()))
            {
                doc.Add(new Paragraph("\nExtracted Text"));
                doc.Add(new Paragraph(extractedText.ToString()));
            }

            doc.Close();
            pdf.Close();
            writer.Close();
        }

        public async Task Run()
        {
            Console.WriteLine("Exercice 2: Analyze an image from a URL with multiple features");
            Console.Write("Enter image URL: ");
            string imageUrl = Console.ReadLine();

            ImageAnalysis analysis = await _client.AnalyzeImageAsync(imageUrl, _features);

            if (analysis.Adult.AdultScore > 0.5)
                Console.WriteLine("WARNING: Adult content detected");

            if (analysis.Adult.RacyScore > 0.5)
                Console.WriteLine("WARNING: Racy content detected");

            await GenerateReport(analysis, imageUrl);
        }

    }
}
