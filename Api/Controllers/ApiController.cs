using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using ZeeAcom.Common;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace ZeeAcom.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Admin)] // the whole API is admin-only for now
    public abstract class ApiController : ControllerBase
    {
        private IMediator mediator;

        protected IMediator Mediator => this.mediator ??= this.HttpContext.RequestServices.GetRequiredService<IMediator>()!;
    }
}
