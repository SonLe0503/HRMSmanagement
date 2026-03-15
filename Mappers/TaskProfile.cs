using AutoMapper;
using HRManagement.DTOs;

namespace HRManagement.Mappers
{
    public class TaskProfile : Profile
    {
        public TaskProfile()
        {
            CreateMap<Models.Task, TaskDTO>()
                .ForMember(dest => dest.AssignedUsername,
                    opt => opt.MapFrom(src => src.AssignedToNavigation.Username));
            CreateMap<CreateTaskDTO, Models.Task>();
            CreateMap<UpdateTaskDTO, Task>()
                .ForAllMembers(opt => opt.Condition(
                    (src, dest, srcMember) => srcMember != null));
        }
    }
}
