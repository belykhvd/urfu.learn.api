// ReSharper disable InconsistentNaming
namespace Core.Repo
{
    public static class PgSchema
    {
        public const string course = "course";              // id, data
        public const string course_index = "course_index";  // id, name

        public const string course_tasks = "course_tasks";  // course_id, task_id

        public const string task = "task";                  // id, data
        public const string task_index = "task_index";      // id, name

        public const string ChallengeAccomplishment = "challenge_accomplishment";

        public const string solution_info = "solution_info";
        public const string solution_index = "solution_index";

        public const string Group = "\"group\"";
        public const string GroupIndex = "group_index";
        public const string GroupMembership = "group_membership";

        public const string Profile = "profile";
        public const string ProfileIndex = "profile_index";

        public const string User = "\"user\"";
        public const string UserIndex = "user_index";
    }
}