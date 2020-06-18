using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Contracts.Types.Auth;
using Contracts.Types.Course;
using Contracts.Types.Task;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace TestDataGenerator
{
    [TestFixture]
    public class Auditor
    {
        private HttpClient httpClient;

        [Test]
        public async Task Run()
        {
            var cookies = new CookieContainer();
            var handler = new HttpClientHandler {CookieContainer = cookies};
            httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost:8080/")
            };

            var doctorWho = new RegistrationData
            {
                Email = "doctor.who@tardis.space",
                Password = "*****",
                Surname = "Doctor",
                FirstName = "Who",
                SecondName = "Tardis",
                Role = UserRole.Admin
            };

            //var authResult = Post<AuthResult>("signUp", doctorWho, HttpStatusCode.OK);

            Post<AuthResult>("signUp", doctorWho, HttpStatusCode.Conflict);

            var authData = new AuthData
            {
                Email = "doctor.who@tardis.space",
                Password = "*****"
            };

            var authResult = Post<AuthResult>("signIn", authData, HttpStatusCode.OK);
            authResult.Should().NotBeNull();
            //authResult.Fio.Should().Be("Doctor W. T.");
            authResult.Role.Should().Be(UserRole.Admin);
            authResult.UserId.Should().NotBeEmpty();

            var doctorWhoId = authResult.UserId;

            Get<Course>($"course/get?id={Guid.NewGuid()}", HttpStatusCode.NotFound);

            var course = new Course
            {
                Name = "Space-Time travelling",
                DescriptionText = "Geronimo!",
                MaxScore = 1000
            };

            var courseId = Post<Guid>("course/save", course, HttpStatusCode.OK);
            courseId.Should().NotBeEmpty();
            course.Id = courseId;

            Get<Course>($"course/get?id={courseId}", HttpStatusCode.OK).Should().BeEquivalentTo(course);

            var courseTask = new CourseTask
            {
                Name = "Find Gallifrey!",
                DescriptionText = "It is somewhere else",
                Deadline = DateTime.Now,
                MaxScore = 100,
                RequirementList = new[]
                {
                    new RequirementStatus
                    {
                        Text = "Find Doctor!"
                    }
                }
            };

            var taskId = Post<Guid>($"course/addTask?courseId={courseId}", courseTask, HttpStatusCode.OK);
            taskId.Should().NotBeEmpty();
            courseTask.Id = taskId;

            Get<Course>($"task/get?id={taskId}", HttpStatusCode.OK).Should().BeEquivalentTo(courseTask);

            Post<object>($"task/delete?id={taskId}", null, HttpStatusCode.OK);
            Get<Course>($"task/get?id={courseId}", HttpStatusCode.NotFound);

            Post<object>($"course/delete?id={courseId}", null, HttpStatusCode.OK);
            Get<Course>($"course/get?id={courseId}", HttpStatusCode.NotFound);
        }

        private T Get<T>(string url, HttpStatusCode statusCode)
        {
            var response = httpClient.GetAsync(url).Result;
            return ProcessResponse<T>(response, statusCode);
        }

        private T Post<T>(string url, object data, HttpStatusCode statusCode)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync(url, content).Result;
            return ProcessResponse<T>(response, statusCode);
        }

        private static T ProcessResponse<T>(HttpResponseMessage response, HttpStatusCode statusCode)
        {
            response.StatusCode.Should().Be(statusCode);
            if (!response.IsSuccessStatusCode)
                return default;

            using (var mem = new MemoryStream())
            {
                response.Content.CopyToAsync(mem).GetAwaiter().GetResult();
                mem.Seek(0, SeekOrigin.Begin);

                using (var streamReader = new StreamReader(mem))
                {
                    var serializedData = streamReader.ReadToEndAsync().GetAwaiter().GetResult();
                    return JsonConvert.DeserializeObject<T>(serializedData);
                }
            }
        }
    }
}