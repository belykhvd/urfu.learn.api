// ReSharper disable InconsistentNaming
namespace Repo
{
    public static class PgSchema
    {
        public const string auth = nameof(auth);

        public const string course = nameof(course);
        public const string course_index = nameof(course_index);

        public const string course_tasks = nameof(course_tasks);

        public const string task = nameof(task);
        public const string task_index = nameof(task_index);
        public const string task_progress = nameof(task_progress);

        public const string user_profile = nameof(user_profile);
        public const string user_index = nameof(user_index);

        public const string file_index = nameof(file_index);

        public const string invite = nameof(invite);

        public const string group = "\"" + nameof(group) + "\"";
        public const string group_index = nameof(group_index);
        public const string group_membership = nameof(group_membership);

        public const string attachment = nameof(attachment);

        public const string js_test = nameof(js_test);
        public const string js_queue = nameof(js_queue);
        public const string js_check_result = nameof(js_check_result);
    }
}