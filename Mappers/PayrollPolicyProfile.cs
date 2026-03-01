using AutoMapper;
using HRManagement.DTOs;
using HRManagement.Models;

namespace HRManagement.Mappers
{
    public class PayrollPolicyProfile : Profile
    {
        public PayrollPolicyProfile()
        {
            CreateMap<PayrollPolicy, PayrollPolicyListDTO>()
              .ForMember(dest => dest.LastModifiedDate,
                  opt => opt.MapFrom(src => src.ModifiedDate ?? src.CreatedDate));
            CreateMap<CreatePayrollPolicyDTO, PayrollPolicy>();
        }
    }
}
