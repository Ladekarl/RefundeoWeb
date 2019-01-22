using System.Threading.Tasks;

namespace Refundeo.Core.Services.Interfaces
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync(string viewName, object model);
    }
}
