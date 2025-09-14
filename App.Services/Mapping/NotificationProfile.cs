using App.Core.DTOs.Notification;
using App.Core.Models.Notification;
using AutoMapper;
using MongoDB.Bson;

namespace App.Services.Mapping;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<Notification, NotificationDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()));

        CreateMap<NotificationDto, Notification>()
            .ForMember(d => d.Id, o => o.MapFrom(s => ObjectId.Parse(s.Id)));

        CreateMap<NotificationCreateDto, Notification>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore());
        
        CreateMap<NotificationSeen, NotificationSeenDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId.ToString()))
            .ForMember(d => d.NotificationId, o => o.MapFrom(s => s.NotificationId.ToString()));
        
        CreateMap<NotificationSeenDto, NotificationSeen>()
            .ForMember(d => d.Id, o => o.MapFrom(s => ObjectId.Parse(s.Id)))
            .ForMember(d => d.UserId, o => o.MapFrom(s => ObjectId.Parse(s.UserId)))
            .ForMember(d => d.NotificationId, o => o.MapFrom(s => ObjectId.Parse(s.NotificationId)));
        
        CreateMap<NotificationSeenCreateDto, NotificationSeen>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.MapFrom(s => ObjectId.Parse(s.UserId)))
            .ForMember(d => d.NotificationId, o => o.MapFrom(s => ObjectId.Parse(s.NotificationId)))
            .ForMember(d => d.SeenAt, o => o.Ignore());
    }
}