using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZeeAcom.Api.Helpers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.EntityModels;
using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.Core.Owners;

namespace ZeeAcom.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerController : ApiController
    {
        [HttpPost]
        [ProducesResponseType(typeof(OwnerModel), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Result<OwnerModel>>> Add([FromBody] CreateOwnerModel model, CancellationToken cancellationToken = default)
            => ResponseHelper.ResponseOutcome(await this.Mediator.Send(new AddOwnerCommand() { Data = model},cancellationToken),this);
    }
}
