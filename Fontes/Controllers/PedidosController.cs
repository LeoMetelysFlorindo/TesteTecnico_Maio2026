using Microsoft.AspNetCore.Mvc;
using PedioApi.DTOs;
using PedioApi.models;
using PedioApi.Services;
using Serilog.Core;

namespace ProductApi.Controllers;


[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PedidosController : ControllerBase
{
    private readonly IPedidoService _pedidoService;

    private readonly ILogger<PedidosController> _logger;

    public PedidosController(IPedidoService productService, ILogger<PedidosController> logger)
    {
        _pedidoService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the list of products with pagination and filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<PedidoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,       
        [FromQuery] StatusPedido? status = null)
    {
        // Validate pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var (items, totalCount) = await _pedidoService.GetAllAsync(
            page, pageSize, search, status);

        _logger.LogInformation("Criando pedido para cliente: {Cliente}");

        // Standardized paginated response
        var response = new PaginatedResponse<PedidoDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return Ok(response);
    }

    /// <summary>
    /// Retrieves a product by its identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _pedidoService.GetByIdAsync(id);

        _logger.LogInformation("Listando dados do Pedido: ", id);

        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found." });
        }

        return Ok(product);
    }


    // =========================
    // CREATE PEDIDO
    // =========================
    /// <summary>
    /// Creates a new product.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePedidoDto dto)
    {
        // Validation is automatic via FluentValidation
        var product = await _pedidoService.CreateAsync(dto);

        _logger.LogInformation("Criando Pedido: ", product.Id);

        // Returns 201 with the created resource URL
        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product);
    }

    // =========================
    // UPDATE PEDIDO
    // =========================
    /// <summary>
    /// Updates an existing product.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePedidoDto dto)
    {
        var product = await _pedidoService.UpdateAsync(id, dto);

        _logger.LogInformation("Alterando o Pedido: ", product.Id);

        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found." });
        }

        return Ok(product);
    }

    // =========================
    // DELETAR PEDIDO
    // =========================
    /// <summary>
    /// Deletes a product.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _pedidoService.DeleteAsync(id);

        _logger.LogInformation("Deletando Pedido: ", id);

        if (!deleted)
        {
            return NotFound(new { message = $"Product with ID {id} not found." });
        }

        // 204 No Content for successful deletion
        return NoContent();
    }

    // =========================
    // CANCELAR PEDIDO
    // =========================
    [HttpPut("{id}/cancelar")]
    [ProducesResponseType(typeof(PedidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        try
        {
            var result = await _pedidoService.CancelarAsync(id);

            _logger.LogInformation("Cancelando Pedido: ", id);

            if (result == null)
                return NotFound(new { message = $"Pedido {id} não encontrado." });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

