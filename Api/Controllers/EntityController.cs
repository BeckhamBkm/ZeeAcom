using Microsoft.AspNetCore.Mvc;
using ZeeAcom.Api.Helpers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.EntityModels;
using ZeeAcom.Core.Entities.Commands;

namespace ZeeAcom.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntityController : ApiController
    {

        //[HttpPost("Search")]
        //[ProducesResponseType(typeof(Result<IEnumerable<EntityModel>>), 200)]
        //public async Task<ActionResult<Result<IEnumerable<EntityModel>>>> Search(CancellationToken cancellationToken = default)
        //    => ResponseHelper.ResponseOutcome(await Mediator.Send());


        [HttpPost]
        [ProducesResponseType(typeof(EntityModel),200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Result<EntityModel>>> Add([FromBody] CreateEntityModel model, CancellationToken cancellationToken = default)
            => ResponseHelper.ResponseOutcome(await this.Mediator.Send(new AddEntityCommand() { Data = model }, cancellationToken), this);
    }
}
