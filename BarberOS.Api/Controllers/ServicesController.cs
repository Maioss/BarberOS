using BarberOS.Api.Common;
using BarberOS.Application.Services.DTOs;
using BarberOS.Application.Services.UseCases;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberOS.Api.Controllers
{
    [ApiController]
    [Route("api/services")]
    [Authorize(Policy = Policies.Management)]
    public class ServicesController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateServiceRequest request,
            [FromServices] IValidator<CreateServiceRequest> validator,
            [FromServices] CreateServiceUseCase useCase,
            CancellationToken ct)
        {
            await validator.ValidateAndThrowAsync(request, ct);
            var result = await useCase.ExecuteAsync(request, ct);
            return Created($"/api/services/{result.Id}", ApiResponse<ServiceDto>.Ok(result, "Servicio creado."));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateServiceRequest request,
            [FromServices] IValidator<UpdateServiceRequest> validator,
            [FromServices] UpdateServiceUseCase useCase,
            CancellationToken ct)
        {
            await validator.ValidateAndThrowAsync(request, ct);
            var result = await useCase.ExecuteAsync(id, request, ct);
            return Ok(ApiResponse<ServiceDto>.Ok(result, "Servicio actualizado."));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(
            Guid id,
            [FromServices] DeleteServiceUseCase useCase,
            CancellationToken ct)
        {
            await useCase.ExecuteAsync(id, ct);
            return NoContent();
        }
    }
}
