using EmployeeInvestigationSystem.Application.DTOs;
using EmployeeInvestigationSystem.Domain.Entities;
using EmployeeInvestigationSystem.Domain.Enums;
using AutoMapper;

namespace EmployeeInvestigationSystem.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Employee mappings
        CreateMap<Employee, EmployeeDto>();
        CreateMap<CreateEmployeeDto, Employee>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        CreateMap<UpdateEmployeeDto, Employee>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        // Investigation mappings
        CreateMap<Investigation, InvestigationDto>();
        CreateMap<InvestigationRemark, InvestigationRemarkDto>();
        CreateMap<InvestigationAttachment, InvestigationAttachmentDto>();
        CreateMap<WarningLetter, WarningLetterDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.Name : null))
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.LetterContent))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.IssuedAt));

        // Leave request mappings
        CreateMap<LeaveAttachment, FileAttachmentDto>();
        CreateMap<LeaveRequest, LeaveRequestDto>()
            .ForMember(dest => dest.LeaveId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));

        // Audit Log mappings
        CreateMap<AuditLog, AuditLogDto>();
        CreateMap<AuditLogDto, AuditLog>();
    }
}
