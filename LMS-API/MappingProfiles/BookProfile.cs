using AutoMapper;
using LMS_API.Models;
using LMS_API.Controllers.Books.Commands;
using LMS_API.Controllers.Books.ViewModels;

namespace LMS_API.MappingProfiles
{
    public class BookProfile : Profile
    {
        public BookProfile()
        {
            CreateMap<CreateBookCommand, Book>()
                .ForMember(dest => dest.UrlPdf, opt => opt.Ignore())
                .ForMember(dest => dest.UrlPic, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

            CreateMap<Book, BookVM>();

            CreateMap<BookTranslation, BookTranslationVM>();
        }
    }
}
