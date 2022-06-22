namespace ReactSpa_Backend.Helpers;

using AutoMapper;
using ReactSpa_Backend.Entities;
using ReactSpa_Backend.Models.Accounts;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<User, AuthenticateResponse>();
        CreateMap<User, AccountResponse>();
        CreateMap<RegisterRequest, User>();


        CreateMap<UpdateRequest, User>()
        .ForAllMembers(opt => opt.Condition(
            (src, dest, prop) =>
            {
                if (prop == null) return false;
                if (prop.GetType() == typeof(string) && string.IsNullOrEmpty((string)prop)) return false;

                if (opt.DestinationMember.Name == "Role" && src.Role == null) return false;

                return true;
            }));
    }
}