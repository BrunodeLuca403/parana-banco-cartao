using Microsoft.AspNetCore.Mvc;
using ParanaBanco.Cartoes.Application.Common;
using ParanaBanco.Cartoes.Application.DTOs;
using ParanaBanco.Cartoes.Application.Services.Interface;

namespace ParanaBanco.Cartoes.API.Controllers
{
    [ApiController]
    [Route("api/v1/cartoes")]
    public class CartoesController : ControllerBase
    {
        private readonly ICartaoService _cartaoService;

        public CartoesController(ICartaoService cartaoService)
        {
            _cartaoService = cartaoService;
        }

        [HttpGet("{clienteId:guid}")]
        public async Task<ActionResult<ResultViewModel<List<CartaoResponse>>>> ListarPorCliente([FromRoute] Guid clienteId)
        {
            var resultado = await _cartaoService.ListarPorClienteAsync(clienteId);

            if (!resultado.IsSuccess)
            {
                return NotFound(ResultViewModel<List<CartaoResponse>>.Failure(resultado.Error));
            }

            return Ok(ResultViewModel<List<CartaoResponse>>.Success(resultado.Value));
        }
    }
}
