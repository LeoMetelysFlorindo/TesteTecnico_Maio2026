using Microsoft.AspNetCore.Mvc;
using PedioApi.DTOs;
using PedioApi.models;

namespace PedioApi.Services;

public interface IPedidoService
{
    // Retrieval with pagination


    Task<(IEnumerable<PedidoDto> Items, int TotalCount)> GetAllAsync(
      int page = 1,
      int pageSize = 10,
      string? search = null,
      StatusPedido? status = null);


    // Retrieval by ID
    Task<PedidoDto?> GetByIdAsync(Guid id);

    // Creation
    Task<PedidoDto> CreateAsync(CreatePedidoDto dto);

    // Update
    Task<PedidoDto?> UpdateAsync(Guid id, UpdatePedidoDto dto);

    // Deletion
    Task<bool> DeleteAsync(Guid id);

    Task<PedidoDto?> CancelarAsync(Guid id);

}