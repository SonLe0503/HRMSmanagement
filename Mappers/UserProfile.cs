using AutoMapper;
using HRManagement.DTOs;
using HRManagement.Models;

namespace HRManagement.Mappers
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserResponseDTO>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                    src.UserRoles.Select(ur => ur.Role.RoleName).ToList()));
        }
    }
}
