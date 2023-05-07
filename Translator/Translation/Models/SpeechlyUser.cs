using System.Collections.Generic;

namespace Translation.Models
{
    public class SpeechlyUser
    {
        public int Id { get; set; }
        public string ObjectId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public int ResellerId { get; set; }
        public Reseller Reseller { get; set; }
        public bool AccountEnabled { get; set; }
        public bool Deleted { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public string UserName
        {
            get
            {
                try
                {
                    return $"{FirstName} {LastName}";
                }
                catch (System.Exception)
                {
                    return "";
                }
            }
        }
    }
    public class UserRole
    {
        public int UserId { get; set; }
        public string RoleName { get; set; } = "";
    }
}
