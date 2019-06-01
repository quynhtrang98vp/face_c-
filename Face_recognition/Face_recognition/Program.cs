using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.IO;
using System.Linq;

namespace Face_recognition
{
    class Program
    {
        static string _faceAPIKey = "33b1eaf1acdf47daae276f6b3011f5b8";
        const string faceEndpoint = "https://eastasia.api.cognitive.microsoft.com";
        const string personGroupId = "myfriends";
        const string friendImageDir = @"D:\picture\APIFace\QuynhTrang";
        static void Main(string[] args)
        {
            var faceClient = new FaceClient(new ApiKeyServiceClientCredentials(_faceAPIKey),
                                            new System.Net.Http.DelegatingHandler[] { });
            faceClient.Endpoint = faceEndpoint;

            //Create a new PersonGroup

            #region tạo group mới, nếu không tạo thì dùng groupid đã có
            var faceId = faceClient.PersonGroup.CreateAsync("myfriends", "My Friends").GetAwaiter(); // không có getResult
            Console.WriteLine(faceId.IsCompleted);
            Console.WriteLine("Create successful");
            #endregion

            #region Tạo một person mới trong group
            //Add a person to person group 
            var studentTrang = faceClient.PersonGroupPerson.CreateAsync(personGroupId, "Nguyen Thi Quynh Trang - 20164169").GetAwaiter().GetResult();
            string trangid = studentTrang.PersonId.ToString();
            Console.WriteLine("id " + trangid);
            #endregion
           
            #region add thêm ảnh vào Person đã có
            //// nếu cần phải add thêm ảnh
            string getTrangid = "ec0aec29-5a6f-465c-b93f-d900820f7a41";
            Person trang = new Person(new Guid(getTrangid));
            Console.WriteLine(trang.PersonId);
            Console.WriteLine("Add successful");

            string[] array = Directory.GetFiles(friendImageDir, "*.jpg");
            for (int i = 0; i < array.Length; i++)
            {
                string image = array[i];
                using (Stream imageStream = File.OpenRead(image))
                {
                    var r = faceClient.PersonGroupPerson.AddFaceFromStreamAsync(personGroupId, studentTrang.PersonId, imageStream);
                    Console.WriteLine(r.Result);
                    Console.WriteLine("Add Face Successful");
                }
            }
            #endregion

            #region training group
            faceClient.PersonGroup.TrainAsync(personGroupId).GetAwaiter().GetResult();
            TrainingStatus training = null;
            while (true)
            {
                training = faceClient.PersonGroup.GetTrainingStatusAsync(personGroupId).GetAwaiter().GetResult();
                Console.WriteLine("Status" + training.Status);
                if (training.Status == TrainingStatusType.Succeeded)
                {
                    break;
                }
            }
            Console.WriteLine("Training Successful");
            #endregion
            

            #region Identify the face
            string testImage = @"D:\picture\Trang\01.jpg";
            using (Stream imageStream = File.OpenRead(testImage))
            {
                var faces = faceClient.Face.DetectWithStreamAsync(imageStream, true).GetAwaiter().GetResult();
                var faceids = faces.Select(e => (Guid)e.FaceId).ToList();
                try
                {
                    var identifyResults = faceClient.Face.IdentifyAsync(faceids, personGroupId).GetAwaiter().GetResult();

                    foreach (var result in identifyResults)
                    {
                        if (result.Candidates.Count == 0)
                            Console.WriteLine("No one identified");
                        else
                        {
                            var candidateId = result.Candidates[0].PersonId;
                            var person = faceClient.PersonGroupPerson.GetAsync(personGroupId, candidateId)
                            .GetAwaiter().GetResult();
                            Console.WriteLine("Identified as {0}", person.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            #endregion

            Console.ReadLine();
        }

    }
}