using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Auth;
using Contracts.Types.Common;
using Contracts.Types.Course;
using Contracts.Types.Group;
using Contracts.Types.Media;
using Contracts.Types.Task;
using Contracts.Types.User;
using Core;
using Core.Repo;
using Core.Services;
using Dapper;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Repo;

namespace TestDataGenerator
{
    [TestFixture]
    public class TestDataGenerator
    {
        private readonly IAuthService authService;
        private readonly ICourseService courseService;
        private readonly ITaskService taskService;
        private readonly FileRepo fileRepo;

        public TestDataGenerator()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            
            var dbStorableTypes = new[]
            {
                typeof(AuthResult),

                typeof(Profile),
                typeof(Group),

                typeof(Course),
                typeof(CourseIndex),

                typeof(CourseTask),
                typeof(Requirement),
                typeof(Requirement[]),
                typeof(RequirementStatus),
                typeof(RequirementStatus[]),
                typeof(TaskProgress),

                typeof(Link),
                typeof(Link[])
            };

            foreach (var type in dbStorableTypes)
                SqlMapper.AddTypeHandler(type, new DapperTypeHandler());
            
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var profileRepo = new ProfileRepo(config);
            authService = new AuthService(config, profileRepo);

            fileRepo = new FileRepo(config);
            taskService = new TaskService(config, fileRepo);
            courseService = new CourseService(config, taskService, fileRepo);
        }

        [Test]
        public async Task Generate()
        {
            var random = new Random();

            var users = new[]
            {
                MakeUser("Иванович Иван Зарубин", "azakov@gmail.com", "12345", "", UserRole.Admin),
                MakeUser("Иванович Иван Зарубин", "krivedina@gmail.com", "12345", "", UserRole.Student),
                MakeUser("Белых Владислав Дмитриевич", "belykhvd@gmail.com", "123", "", UserRole.Admin),
                MakeUser("Александров Александр Александрович", "alex@urfu2.ru", "123", "", UserRole.Admin),
                MakeUser("Кириллов Кирилл Кириллович", "kir@urfu2.ru", "123", "", UserRole.Admin),
                MakeUser("Иванов Иван Иванович", "evan@urfu2.ru", "123", "КН-402", UserRole.Student),
                MakeUser("Петров Петр Петрович", "petr@urfu2.ru", "123", "КН-402", UserRole.Student),
                MakeUser("Васильева Василиса Васильевна", "vas@urfu2.ru", "123", "КН-402", UserRole.Student)
            };

            var userMap = new Dictionary<Guid, AuthResult>();

            foreach (var user in users)
            {
                var authResult = await authService.SignUp(user).ConfigureAwait(false);
                userMap[authResult.UserId] = authResult;
            }

            var commonAdminId = userMap.Keys.First();

            await fileRepo.RegisterAttachment(new Attachment
            {
                Id = Guid.Empty,
                Name = "Ubuntu 16.04.iso",
                Size = 1024 * 1024 * 1024,
                Timestamp = new DateTime(2020, 06, 03),
                Author = commonAdminId
            }).ConfigureAwait(false);

            var courses = new[]
            {
                MakeCourse("JavaScript", 100, new DateTime(2020, 05, 30)),
                MakeCourse("TypeScript", 75, new DateTime(2022, 06, 03)),
                MakeCourse("React.JS", 50),
                MakeCourse("Angular.JS", 100, new DateTime(2020, 06, 05))
            };

            foreach (var course in courses)
            {
                var courseId = await courseService.Save(null, course).ConfigureAwait(false);

                var tasks = new[]
                {
                    MakeTask("First", random.Next(0, 41)),
                    MakeTask("Second", random.Next(0, 41)),
                    MakeTask("Third", random.Next(0, 41))
                };

                foreach (var task in tasks)
                {
                    var taskId = await courseService.AddTask(courseId, task).ConfigureAwait(false);

                    await taskService.RegisterAttachment(taskId, commonAdminId, Guid.Empty, AttachmentType.Input).ConfigureAwait(false);

                    foreach (var userId in userMap.Keys)
                        await taskService.RegisterAttachment(taskId, userId, Guid.Empty, AttachmentType.Solution).ConfigureAwait(false);
                }
            }
        }

        private static Course MakeCourse(string name, int maxScore, DateTime? deadline = null) => new Course
        {
            Name = $"❦ {name} ❦",
            DescriptionText = "Здесь могла быть ваша реклама",
            MaxScore = maxScore
        };

        private static CourseTask MakeTask(string namePrefix, int maxScore) => new CourseTask
        {
            Name = $"💐 {namePrefix} task",
            DescriptionText = $"{namePrefix} description",
            Deadline = new DateTime(2020, 6, 5),
            MaxScore = maxScore,
            RequirementList = new[]
            {
                new RequirementStatus {Id = Guid.NewGuid(), Text = "☆ Requirement 1 ☆"},
                new RequirementStatus {Id = Guid.NewGuid(), Text = "☆ Requirement 2 ☆"},
                new RequirementStatus {Id = Guid.NewGuid(), Text = "☆ Requirement 3 ☆"},
                new RequirementStatus {Id = Guid.NewGuid(), Text = "☆ Requirement 4 ☆"},
                new RequirementStatus {Id = Guid.NewGuid(), Text = "☆ Requirement 5 ☆"}
            }
        };

        private static RegistrationData MakeUser(string fio, string email, string password, string group, UserRole role)
        {
            var splitted = fio.Split();
            return new RegistrationData
            {
                Surname = splitted[0],
                FirstName = splitted[1],
                SecondName = splitted[2],
                Email = email,
                Password = password,
                Group = group,
                Role = role
            };
        }
    }
}