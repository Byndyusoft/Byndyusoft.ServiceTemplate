namespace Byndyusoft.ServiceTemplate.Api.Client.Interfaces
{
    using System.Threading.Tasks;
    using Shared.Dtos;

    public interface ITemplateClient
    {
        public Task<TemplateDto> GetTemplate(int templateId);
    }
}