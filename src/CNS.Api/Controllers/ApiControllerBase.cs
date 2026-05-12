using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CNS.Api.Controllers;

/// <summary>
/// پایهٔ مشترک برای همهٔ کنترلرهای API: JSON، احراز هویت، و قرارداد Web API.
/// </summary>
[ApiController]
//[Authorize]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
}
