using SecuredByEmailIVerification.Model;

namespace SecuredByEmailIVerification.ServiceContracts
{
    public interface IJwtService
    {
        AuthenticationResponse CreateJwtToken(ApplicationUser user);
    }
}
