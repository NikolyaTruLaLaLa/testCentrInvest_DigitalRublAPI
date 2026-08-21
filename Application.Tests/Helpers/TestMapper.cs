using Application.Mapping;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Tests.Helpers
{
    public static class TestMapper
    {
        public static IMapper Create()
        {
            var config = new MapperConfiguration(
                cfg => cfg.AddProfile<MappingProfile>(),
                NullLoggerFactory.Instance
            );
            return config.CreateMapper();
        }
    }
}
