using System;

namespace Core
{
    public static class Constants
    {
        public const long _2GB = 2147483648;
        public const int MultipartBoundaryLengthLimit = 70;

        public const int ChatMessageMaxLength = 1024; 

        public const string ProfessorOrAdmin = "Professor,Admin";

        public const string SolutionUploadError = "Ошибка загрузки решения";

        public const string ErrorCannotDeleteAdminGroup = "Группу администраторов удалить нельзя";
        public const string ErrorCannotDeleteProfessorGroup = "Удалить группу преподавателей может только администратор";

        public static readonly Guid Guid_1 = new Guid("00000000000000000000000000000001");
    }
}