using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System.IO;

namespace ElfDestroyer
{
    class Program
    {
        private static async Task SolveImageAsync(string imagePath, VisionServiceClient visionServiceClient)
        {
            Console.WriteLine(imagePath);
            try
            {
                using (Stream imageFileStream = File.OpenRead(imagePath))
                {
                    //
                    // Analyze the image for all visual features
                    //
                    //Log("Calling VisionServiceClient.AnalyzeImageAsync()...");
                    int indexBestTag, i, length;
                    string directoryPath = Path.GetDirectoryName(imagePath);
                    string imageName = Path.GetFileName(imagePath);
                    VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Tags };
                    AnalysisResult analysisResult = await visionServiceClient.AnalyzeImageAsync(imageFileStream, visualFeatures);
                    length = analysisResult.Tags.Length;
                    if (length == 0)
                    {
                        directoryPath = System.IO.Path.Combine(directoryPath, "Tagless");
                    }
                    else
                    {
                        indexBestTag = 0;
                        for (i = 1; i < length; ++i)
                        {
                            if (analysisResult.Tags[i].Confidence > analysisResult.Tags[indexBestTag].Confidence)
                            {
                                indexBestTag = i;
                            }
                        }
                        directoryPath = System.IO.Path.Combine(directoryPath, analysisResult.Tags[indexBestTag].Name);
                    }
                    if (!System.IO.Directory.Exists(directoryPath))
                    {
                        System.IO.Directory.CreateDirectory(directoryPath);
                    }
                    System.IO.File.Copy(imagePath, System.IO.Path.Combine(directoryPath, imageName));
                }
            } catch(Microsoft.ProjectOxford.Vision.ClientException e)
            {
                Console.WriteLine("-->Problems with Vision API were encountered while processing " + imagePath + ":");
                Console.WriteLine(e.Message);
            } catch(Exception e)
            {
                Console.WriteLine("-->Some sort of problems (IO probably) were encountered while processing " + imagePath + ":");
            }
}
        public static void Main(string[] args)
        {
            string imagesDirectoryPath = @"C:\_MAC\Programare\MIC Jam Cognitive recognition\ElfDestroyer\ElfDestroyer\bin\Debug\Images";
            VisionServiceClient visionServiceClient = new VisionServiceClient("1f1631a6e7924f67b92362803f8bbced", "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0");
            Task.Run(async () =>
            {
                foreach (string image in Directory.EnumerateFiles(imagesDirectoryPath))
                {
                    await SolveImageAsync(image, visionServiceClient);
                }
            }).GetAwaiter().GetResult();
        }
    }
}
