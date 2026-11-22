using Microsoft.AspNetCore.Mvc;
using ZeeAcom.Api.Helpers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.RoomModels;
using ZeeAcom.Core.Rooms.Commands;
using ZeeAcom.Core.Rooms.Queries;

namespace ZeeAcom.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ApiController
    {
        [HttpPost("search")]
        [ProducesResponseType(typeof(Result<IEnumerable<RoomModel>>), 200)]
        public async Task<ActionResult<Result<IEnumerable<RoomModel>>>> Search([FromBody] SearchRoomModel model, CancellationToken cancellationToken = default)
            => ResponseHelper.ResponseOutcome(await this.Mediator.Send(new GetRoomsQuery { Data = model }, cancellationToken), this);

        [HttpPost("{propertyId}")]
        [ProducesResponseType(typeof(Result<RoomModel>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Result<RoomModel>>> Add(Guid propertyId, [FromBody] SaveRoomModel model, CancellationToken cancellationToken = default)
            => ResponseHelper.ResponseOutcome(await this.Mediator.Send(new AddRoomCommand { PropertyId = propertyId, Data = model }, cancellationToken), this);

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Result<RoomModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Result<RoomModel>>> Update(Guid id, [FromBody] SaveRoomModel model, CancellationToken cancellationToken = default)
            => ResponseHelper.ResponseOutcome(await this.Mediator.Send(new UpdateRoomCommand { Id = id, Data = model }, cancellationToken), this);

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Result), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Result>> Delete(Guid id, CancellationToken cancellationToken = default)
            => ResponseHelper.ResponseOutcome(await this.Mediator.Send(new DeleteRoomCommand { Id = id }, cancellationToken), this);
    }
}
