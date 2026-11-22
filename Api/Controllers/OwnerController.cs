using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZeeAcom.Api.Helpers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.PropertyModels;
using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.Core.Owners;
using ZeeAcom.Core.Owners.Commands;
using ZeeAcom.Core.Owners.Queries;

namespace ZeeAcom.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerController : ApiController
    {
        [HttpPost("search")]
        public async Task<ActionResult<Result<IEnumerable<OwnerModel>>>> Search(SearchOwnerModel model, CancellationToken cancellationToken = default)
            => ResponseHelper.ResponseOutcome(await this.Mediator.Send(new GetOwnersQuery(){Data = model}, cancellationToken), this);


        [HttpPost]
        [ProducesResponseType(typeof(OwnerModel), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Result<OwnerModel>>> Add([FromBody] CreateOwnerModel model, CancellationToken cancellationToken = default)
            => ResponseHelper.ResponseOutcome(await this.Mediator.Send(new AddOwnerCommand() { Data = model},cancellationToken),this);


        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Result),200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Result>> Delete(Guid id, CancellationToken cancellationToken = default)
            => ResponseHelper.ResponseOutcome(await this.Mediator.Send(new DeleteOwnerCommand{ Id = id},cancellationToken),this);
    }
}
