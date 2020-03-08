using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    public class CrudControllerBase<TEntity> : ControllerBase where TEntity : ICrudEntity
    {
        private readonly string rootRoute;
        
        protected CrudControllerBase(string rootRoute)
        {
            this.rootRoute = rootRoute;
        }
        
        // [HttpPost]
        // [Route()]
        // protected async Task<IActionResult> CreateEntity([FromBody] TEntity data)
        // {
        //     
        // }
    }
}