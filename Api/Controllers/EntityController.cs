using Microsoft.AspNetCore.Mvc;
using ZeeAcom.Api.Helpers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.EntityModels;
using ZeeAcom.Core.Entities.Commands;
using ZeeAcom.Core.Entities.Queries;

namespace ZeeAcom.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntityController : ApiController
    {

        [HttpPost("search")]
        [ProducesResponseType(typeof(Result<IEnumerable<EntityModel>>), 200)]
        public async Task<ActionResult<Result<IEnumerable<EntityModel>>>> Search([FromBody] SearchEntityModel model, CancellationToken cancellationToken = default)
            => ResponseHelper.ResponseOutcome(await this.Mediator.Send(new GetEntitiesQuery { Data = model }, cancellationToken), this);

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Result), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Result>> Delete(Guid id, CancellationToken cancellationToken = default)
            => ResponseHelper.ResponseOutcome(await this.Mediator.Send(new DeleteEntityCommand { Id = id }, cancellationToken), this);


        [HttpPost]
        [ProducesResponseType(typeof(EntityModel),200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Result<EntityModel>>> Add([FromBody] CreateEntityModel model, CancellationToken cancellationToken = default)
            => ResponseHelper.ResponseOutcome(await this.Mediator.Send(new AddEntityCommand() { Data = model }, cancellationToken), this);
    }
}
