using Microsoft.AspNetCore.Mvc;
using Shared.Common;
using System.Threading.Tasks;

namespace SortSchedule.Controllers.Common
{
    public abstract class BaseController : ControllerBase
    {
        protected async Task<IActionResult> HandleActionAsync<T>(Task<Result<T>> task)
        {
            var res = await task;
            return StatusCode((int)res.StatusCode, res);
        
        }
    }
}
