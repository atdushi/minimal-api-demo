using MagicVilla_CouponAPI.Models.DTO;

namespace MagicVilla_CouponAPI.Repository.IRepository
{
    public interface IAuthRepository
    {
        bool IsUniqueUser(string username);
        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequest);
        Task<UserDTO> Register(RegistrationRequestDTO registrationRequest);
    }
}
