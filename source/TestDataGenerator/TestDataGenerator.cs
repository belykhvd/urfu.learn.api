using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Contracts.Services;
using Contracts.Types.Auth;
using Contracts.Types.Course;
using Contracts.Types.Group;
using Contracts.Types.Media;
using Contracts.Types.Task;
using Core.Repo;
using Core.Services;
using Dapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Repo;

namespace TestDataGenerator
{
    [TestFixture]
    public class TestDataGenerator
    {
        private readonly IAuthService authService;
        private readonly IUserService userService;
        private readonly ICourseService courseService;
        private readonly ITaskService taskService;
        private readonly IGroupService groupService;
        private readonly FileRepo fileRepo;

        public TestDataGenerator()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            var contractsAssembly = Assembly.GetAssembly(typeof(AuthData));
            foreach (var type in contractsAssembly.DefinedTypes)
            {
                if (type.IsClass && !type.ContainsGenericParameters)
                    SqlMapper.AddTypeHandler(type.AsType(), new DapperTypeHandler());
            }

            var arrayTypes = new[]
            {
                typeof(Requirement[]),
                typeof(RequirementStatus[])
            };
    
            foreach (var type in arrayTypes)
                SqlMapper.AddTypeHandler(type, new DapperTypeHandler());

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var profileRepo = new ProfileRepo(config);
            authService = new AuthService(config, profileRepo);

            fileRepo = new FileRepo(config);
            taskService = new TaskService(config, fileRepo);
            courseService = new CourseService(config, taskService);
            groupService = new GroupService(config);

            userService = new UserService(config, profileRepo);
        }

        private async Task InitializeAdministrators()
        {
            var administratorGroup = new Group
            {
                Id = Guid.Empty,
                Name = "Администраторы"
            };

            await groupService.Save(administratorGroup).ConfigureAwait(false);

            var administrators = new[]
            {
                MakeUser("Казаков Михаил Альбертович", "azakov31@gmail.com", "12345", "Администраторы", UserRole.Professor),
                MakeUser("Белых Владислав Дмитриевич", "belykhvd@gmail.com", "12345", "Администраторы", UserRole.Professor)
            };

            foreach (var admin in administrators)
            {
                var authResult = await authService.SignUp(admin).ConfigureAwait(false);
                if (authResult == null)
                    continue;

                await groupService.InviteStudent(Guid.Empty, admin.Email, false).ConfigureAwait(false);
                //await groupService.AcceptInvite(Guid.Empty, authResult.UserId).ConfigureAwait(false);
            }
        }

        private async Task InitializeProfessors()
        {
            var professorsGroup = new Group
            {
                Id = Guid.Parse(Guid.Empty.ToString("N").Substring(0, 31) + "1"),
                Name = "Преподаватели"
            };

            await groupService.Save(professorsGroup).ConfigureAwait(false);

            var professors = new[]
            {
                MakeUser("Ландау Лев Давидович", "landau@urfu2.ru", "12345", "Преподаватели", UserRole.Admin)
            };

            foreach (var professor in professors)
            {
                var authResult = await authService.SignUp(professor).ConfigureAwait(false);
                if (authResult == null)
                    continue;

                await groupService.InviteStudent(Guid.Parse(Guid.Empty.ToString("N").Substring(0, 31) + "1"), professor.Email, false).ConfigureAwait(false);
                //await groupService.AcceptInvite(Guid.Empty, authResult.UserId).ConfigureAwait(false);
            }
        }

        [Test]
        public async Task Generate()
        {
            await InitializeAdministrators().ConfigureAwait(false);
            await InitializeProfessors().ConfigureAwait(false);

            var random = new Random();

            var users = new[]
            {
                MakeUser("Иванович Иван Зарубин", "azakov@gmail.com", "12345", "", UserRole.Admin),
                MakeUser("Иванович Иван Зарубин", "krivedina@gmail.com", "12345", "", UserRole.Student),
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
                if (authResult == null)
                {
                    authResult = await authService.Authorize(new AuthData
                    {
                        Email = user.Email,
                        Password = user.Password
                    }).ConfigureAwait(false);
                }

                authResult.Should().NotBeNull();

                userMap[authResult.UserId] = authResult;
            }

            var commonAdminId = userMap.Keys.First();

            var commonAttachment = new Attachment
            {
                Id = Guid.Empty,
                Name = "Ubuntu 16.04.iso",
                Size = 1024 * 1024 * 1024,
                Timestamp = new DateTime(2020, 06, 03),
                Author = commonAdminId
            };
            //await fileRepo.RegisterFile(commonAttachment).ConfigureAwait(false);

            var courses = new[]
            {
                MakeCourse("JavaScript", 100),
                MakeCourse("TypeScript", 75),
                MakeCourse("React.JS", 50),
                MakeCourse("Angular.JS", 100)
            };

            foreach (var course in courses)
            {
                var courseId = await courseService.Save(course).ConfigureAwait(false);
                (await courseService.Get(courseId).ConfigureAwait(false)).Should().BeEquivalentTo(course);

                var tasks = new[]
                {
                    MakeTask("Простая", random.Next(0, 41)),
                    MakeTask("Сложная", random.Next(0, 41)),
                    MakeTask("Веселая", random.Next(0, 41))
                };

                foreach (var task in tasks)
                {
                    var taskId = await courseService.AddTask(courseId, task).ConfigureAwait(false);
                    (await taskService.Get(taskId).ConfigureAwait(false)).Should().BeEquivalentTo(task);

                    await taskService.RegisterAttachment(taskId, commonAdminId, Guid.Empty, AttachmentType.Input).ConfigureAwait(false);
                    //(await taskService.GetInputAttachment(taskId).ConfigureAwait(false)).Should().BeEquivalentTo(commonAttachment);

                    foreach (var userId in userMap.Keys)
                    {
                        await taskService.RegisterAttachment(taskId, userId, Guid.Empty, AttachmentType.Solution).ConfigureAwait(false);
                        //(await taskService.GetSolutionAttachment(taskId, userId).ConfigureAwait(false)).Should().BeEquivalentTo(commonAttachment);
                    }
                }
            }

            var groups = new[]
            {
                MakeGroup("КН-401"),
                MakeGroup("КН-402"),
                MakeGroup("КН-403")
            };

            //(await groupService.GetInviteList().ConfigureAwait(false)).Should().BeEquivalentTo();
            //(await groupService.GetStudentList().ConfigureAwait(false)).Should().BeEquivalentTo();
            
            foreach (var group in groups)
            {
                await groupService.Save(group.Id, group).ConfigureAwait(false);
                (await groupService.Get(group.Id).ConfigureAwait(false)).Should().BeEquivalentTo(group);
            }

            var firstGroup = groups.First();

            foreach (var student in users.Where(x => x.Role == UserRole.Student))
                await groupService.InviteStudent(firstGroup.Id, student.Email).ConfigureAwait(false);
        }

        private static Course MakeCourse(string name, int maxScore) => new Course
        {
            Name = $"{name}",
            DescriptionText = "Здесь могла быть ваша реклама",
            MaxScore = maxScore
        };

        private static CourseTask MakeTask(string namePrefix, int maxScore) => new CourseTask
        {
            Name = $"{namePrefix} задача",
            DescriptionText = $"{namePrefix} описание",
            Deadline = new DateTime(2020, 6, 5),
            MaxScore = maxScore,
            RequirementList = new[]
            {
                new RequirementStatus {Id = Guid.NewGuid(), Text = "По времени O(logN)"},
                new RequirementStatus {Id = Guid.NewGuid(), Text = "По памяти O(N)"},
                new RequirementStatus {Id = Guid.NewGuid(), Text = "Не более 100 строк кода"},
                new RequirementStatus {Id = Guid.NewGuid(), Text = "С комментариями"},
                new RequirementStatus {Id = Guid.NewGuid(), Text = "CodeStyle УрФУ"}
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

        private static Group MakeGroup(string name) => new Group
        {
            Id = Guid.NewGuid(),
            Name = name,
            Year = 2020
        };
    }
}