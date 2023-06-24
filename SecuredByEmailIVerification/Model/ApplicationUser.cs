using Microsoft.AspNetCore.Identity;

namespace SecuredByEmailIVerification.Model
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? PersonName { get; set; }

    }
}
