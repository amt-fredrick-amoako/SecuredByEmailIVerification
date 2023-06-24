using System.ComponentModel.DataAnnotations;

namespace SecuredByEmailIVerification.Model
{
    public class User
    {
        [Key] 
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string? Name { get; set; }
        public string? Country { get; set; }
        public string? Occupation { get; set; }
    }
}
