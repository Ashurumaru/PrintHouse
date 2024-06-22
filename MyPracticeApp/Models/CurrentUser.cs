using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPracticeApp.Models
{
    public class CurrentUser
    {
        public static int EmployeeId { get; set; }
        public static string FirstName { get; set; }
        public static string Patronymic { get; set; }
        public static string LastName { get; set; }
        public static int PositionId { get; set; }
        public static string Phone { get; set; }
        public static string Login { get; set; }
        public static string Password { get; set; }
        public static string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
