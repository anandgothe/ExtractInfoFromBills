using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;

namespace MyDemo1
{
    class Program
    {
        const string cognitiveServicesKey = "5d1XXXXXXc67"; // replace with your key
        const string cognitiveServicesUrl = "https://centralindia.api.cognitive.microsoft.com/";

        static void Main(string[] args)
        {
            var client = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(cognitiveServicesKey));
            client.Endpoint = cognitiveServicesUrl;

            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fileName = Path.Combine(location, "mybill.jpg");
            var fileToProcess = File.OpenRead(fileName);
            var apiResult = client.RecognizePrintedTextInStreamAsync(false, fileToProcess);
            apiResult.Wait();

            var ocrResult = apiResult.Result;

            var words = new Dictionary<System.Drawing.Point, string>();

            foreach (var r in ocrResult.Regions)
            {
                foreach (var l in r.Lines)
                {
                    foreach (var w in l.Words)
                    {
                        var coordinates = w.BoundingBox.Split(',');

                        //Console.WriteLine($"Found word {w.Text} at" +
                        //$" X {coordinates[0]} and Y {coordinates[1]}");

                        words.Add(new System.Drawing.Point
                        {
                            X = int.Parse(coordinates[0]),
                            Y = int.Parse(coordinates[1])
                        }, w.Text);
                    }
                }
            }

            var dateWord = words.FirstOrDefault(x => x.Value == "Date");
            int yOffset = 6; // the Y value can vary slightly for words on the same line
            int lookupX = dateWord.Key.X;
            int lookupY1 = dateWord.Key.Y - yOffset;
            int lookupY2 = dateWord.Key.Y + yOffset;

            var nearestWord = words.Where(w => w.Key.X > lookupX
                        && w.Key.Y >= lookupY1 && w.Key.Y <= lookupY2).FirstOrDefault();

            Console.WriteLine("Date found: " + nearestWord.Value);
        }
    }
}

