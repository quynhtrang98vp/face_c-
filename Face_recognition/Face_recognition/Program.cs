using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Face_recognition
{
    class Program
    {
        static string faceAPIkey = "6b90778a91a34a1cac88c01c912dc579";
        const string faceEndpoint = "https://southeastasia.api.cognitive.microsoft.com/face/v1.0";
        const string personGroudId = "face";
        const string Sinh_vien_img_dir = @"D:\picture\QuynhTrang";
        static void Main(string[] args)
        {
            var faceClient = new FaceClient(new ApiKeyServiceClientCredentials(faceAPIkey),
                                              new System.Net.Http.DelegatingHandler[] { });
            faceClient.Endpoint = faceEndpoint;

            try
            {
                //Create a new PersonGroup
                var faceId = faceClient.PersonGroup.CreateAsync("sinhVien", "SinhVien");
                Console.WriteLine("Create personGroup");


                //add person to personGroup
                string personName = "QuynhTrang";
                var Quynh_Trang = faceClient.PersonGroupPerson.CreateAsync(personGroudId, personName).GetAwaiter().GetResult();
                Console.WriteLine("Add person to personGroup");

                //Add Face
                foreach (var img in Directory.GetFiles(Sinh_vien_img_dir, "*.*"))
                {
                    using (Stream imageStream = File.OpenRead(img))
                    {
                        faceClient.PersonGroupPerson.AddFaceFromStreamAsync(personGroudId, Quynh_Trang.PersonId, imageStream).GetAwaiter().GetResult();
                        Console.WriteLine("Add Person to PersonGroup successful");

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR");
            }

            //trainning
            faceClient.PersonGroup.TrainAsync(personGroudId);
            Console.WriteLine("training successfull");


            //Identify the face
            string testImg = @"D:\picture\my.jpg";
            using (Stream imgStream = File.OpenRead(testImg))
            {
                var faces = faceClient.Face.DetectWithStreamAsync(imgStream, true).GetAwaiter().GetResult();
                var faceids = faces.Select(e => (Guid)e.FaceId).ToArray();
                var identifyResults = faceClient.Face.IdentifyAsync(faceids, personGroudId).GetAwaiter().GetResult();
                try
                {
                    
                    foreach (var result in identifyResults)
                    {
                        if (result.Candidates.Count == 0)
                        {
                            Console.WriteLine("No one identified");
                        }
                        else
                        {

                            var candidateId = result.Candidates[0].PersonId;
                            var person = faceClient.PersonGroupPerson.GetAsync(personGroudId, candidateId).GetAwaiter().GetResult();
                            Console.WriteLine("Identified as {0}", person.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR");

                }

            }
        }
    }
}
