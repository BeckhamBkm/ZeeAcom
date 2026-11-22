using Microsoft.AspNetCore.Mvc;
using ZeeAcom.Api.Helpers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.PropertyModels;
using ZeeAcom.Core.Properties.Commands;
using ZeeAcom.Core.Properties.Queries;

namespace ZeeAcom.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertyController : ApiController
    {

        [HttpPost("search")]
        [ProducesResponseType(typeof(Result<IEnumerable<PropertyModel>>), 200)]
        public async Task<ActionResult<Result<IEnumerable<PropertyModel>>>> Search([FromBody] SearchPropertyModel model, CancellationToken cancellationToken = default)
            => ResponseHelper.ResponseOutcome(await this.Mediator.Send(new GetPropertiesQuery { Data = model }, cancellationToken), this);

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Result), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Result>> Delete(Guid id, CancellationToken cancellationToken = default)
            => ResponseHelper.ResponseOutcome(await this.Mediator.Send(new DeletePropertyCommand { Id = id }, cancellationToken), this);


        [HttpPost]
        [ProducesResponseType(typeof(PropertyModel),200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Result<PropertyModel>>> Add([FromBody] CreatePropertyModel model, CancellationToken cancellationToken = default)
            => ResponseHelper.ResponseOutcome(await this.Mediator.Send(new AddPropertyCommand() { Data = model }, cancellationToken), this);
    }
}
