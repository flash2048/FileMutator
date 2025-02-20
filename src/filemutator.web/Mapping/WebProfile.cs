using System.Text;
using AutoMapper;
using FileMutator.infrastructure.EF.Entities;
using FileMutator.infrastructure.Models;

namespace FileMutator.Web.Mapping
{
    public class WebProfile : Profile
    {
        public WebProfile()
        {
            FilesMappings();
        }

        private void FilesMappings()
        {
            CreateMap<FileEntity, FileInfoShort>();

            CreateMap<FileEntity, FileInfoFull>()
                .IncludeBase<FileEntity, FileInfoShort>()
                .ForMember(src => src.FileText, dst => dst.MapFrom(src => Encoding.UTF8.GetString(src.FileData)));

            CreateMap<IFormFile, FileEntity>()
                .ForMember(src => src.Name, dst => dst.Ignore())
                .ConstructUsing(FormFileToFileEntityConverter);
        }

        private FileEntity FormFileToFileEntityConverter(IFormFile file, ResolutionContext context)
        {
            var fileEntity = new FileEntity
            {
                Size = file.Length,
                Name = file.FileName,
                ContentType = file.ContentType
            };
            using var memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);
            fileEntity.FileData = memoryStream.ToArray();

            return fileEntity;
        }
    }
}
