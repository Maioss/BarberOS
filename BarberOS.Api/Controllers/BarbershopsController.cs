using BarberOS.Api.Common;
using BarberOS.Application.Barbershops.DTOs;
using BarberOS.Application.Barbershops.UseCases;
using BarberOS.Application.Barbers.DTOs;
using BarberOS.Application.Barbers.UseCases;
using BarberOS.Application.Services.DTOs;
using BarberOS.Application.Services.UseCases;
using BarberOS.Application.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberOS.Api.Controllers
{
    [ApiController]
    [Route("api/barbershops")]
    [Authorize]
    public class BarbershopsController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<BarbershopDto>>>> List(
            [FromQuery] BarbershopFilter filter,
            [FromServices] ListBarbershopsUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(filter, ct);
            return Ok(ApiResponse<PagedResult<BarbershopDto>>.Ok(result));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<BarbershopDto>>> GetById(
            Guid id,
            [FromServices] GetBarbershopByIdUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(id, ct);
            return Ok(ApiResponse<BarbershopDto>.Ok(result));
        }

        [HttpGet("{id:guid}/branches")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<BarbershopDto>>>> ListBranches(
            Guid id,
            [FromServices] ListBranchesUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(id, ct);
            return Ok(ApiResponse<IReadOnlyList<BarbershopDto>>.Ok(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<BarbershopDto>>> Create(
            [FromBody] CreateBarbershopRequest request,
            [FromServices] CreateBarbershopUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<BarbershopDto>.Ok(result, "Barbershop creado exitosamente."));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<BarbershopDto>>> Update(
            Guid id,
            [FromBody] UpdateBarbershopRequest request,
            [FromServices] UpdateBarbershopUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(id, request, ct);
            return Ok(ApiResponse<BarbershopDto>.Ok(result, "Barbershop actualizado exitosamente."));
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(
            Guid id,
            [FromServices] DeleteBarbershopUseCase useCase,
            CancellationToken ct)
        {
            await useCase.ExecuteAsync(id, ct);
            return Ok(ApiResponse<object>.Ok(null!, "Barbershop desactivado exitosamente."));
        }

        [HttpGet("{id:guid}/services")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<ServiceDto>>>> GetServices(
            Guid id,
            [FromServices] ListServicesByBarbershopUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(id, ct);
            return Ok(ApiResponse<IReadOnlyList<ServiceDto>>.Ok(result));
        }

        [HttpGet("{id:guid}/barbers")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<BarberDto>>>> GetBarbers(
            Guid id,
            [FromServices] ListBarbersByBarbershopUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(id, ct);
            return Ok(ApiResponse<IReadOnlyList<BarberDto>>.Ok(result));
        }
    }
}
