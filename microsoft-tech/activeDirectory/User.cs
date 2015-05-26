using System;

namespace TestAD.Entities
{
	
    public class User
    {
        public Int64 EmployeeId { get; set; }
        public String UserName { get; set; }
        public String DisplayName { get; set; }
        public String Name { get; set; }
        public String LastName { get; set; }
        public String Description { get; set; }
        public String Department { get; set; }
        public String EMail { get; set; }
        public String Phone { get; set; }
        public Manager Supervisor { get; set; }
    }

    public struct Manager
    {
        public int EmployeeId { get; set; }
        public String Name { get; set; }
    }
    
}
