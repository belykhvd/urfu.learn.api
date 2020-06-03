// ReSharper disable InconsistentNaming
namespace Core.Repo
{
    public static class PgSchema
    {
        public const string auth = nameof(auth); // email, password_hash, user_id, role

        public const string course = nameof(course);              // id, data
        public const string course_index = nameof(course_index);  // id, name

        public const string course_tasks = nameof(course_tasks);  // course_id, task_id

        public const string task = nameof(task);                    // id, data
        public const string task_index = nameof(task_index);        // id, name, max_score, requirements
        public const string task_progress = nameof(task_progress);  // user_id, task_id, score, done

        public const string user_profile = nameof(user_profile);  // id, data
        public const string user_index = nameof(user_index);  // id, fio

        public const string file_index = nameof(file_index); // id, name, size, timestamp, author

        public const string invite = nameof(invite);

        public const string group = "\"" + nameof(group) + "\"";
        public const string group_index = nameof(group_index);
        public const string group_membership = nameof(group_membership);

        public const string attachment = nameof(attachment);
    }
}