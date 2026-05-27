using Microsoft.EntityFrameworkCore;
using PedioApi.Data;
using PedioApi.DTOs;
using PedioApi.models;
using ProductApi.Controllers;

namespace PedioApi.Services;

public class PedidoService : IPedidoService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PedidoService> _logger;

    public PedidoService(AppDbContext context, ILogger<PedidoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // =========================
    // GET ALL
    // =========================
    public async Task<(IEnumerable<PedidoDto> Items, int TotalCount)> GetAllAsync(
    int page = 1,
    int pageSize = 10,
    string? search = null,
    StatusPedido? status = null)
    {
        var query = _context.Pedidos.AsQueryable();

        // filtro por cliente
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.ClienteNome.Contains(search));
        }

        // FILTRO POR STATUS 
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        // total ANTES da paginação
        var totalCount = await query.CountAsync();

        // 📄 paginação + projeção
        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PedidoDto(
                p.Id,
                p.ClienteNome,
                p.DataCriacao,
                p.ValorTotal,
                p.Status.ToString(),
                p.ItensPedido.Select(i => new ItemPedidoDto(
                    i.Id,
                    i.NomeProduto,
                    i.Quantidade,
                    i.ValorUnitario
                )).ToList()
            ))
            .ToListAsync();

        return (items, totalCount);
    }

    // =========================
    // GET BY ID
    // =========================
    public async Task<PedidoDto?> GetByIdAsync(Guid id)
    {
        var product = await _context.Pedidos
            .Include(p => p.ItensPedido)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return null;

        return new PedidoDto(
            product.Id,
            product.ClienteNome,
            product.DataCriacao,
            product.ValorTotal,
            product.Status.ToString(),
            product.ItensPedido.Select(i => new ItemPedidoDto(
                i.Id,
                i.NomeProduto,
                i.Quantidade,
                i.ValorUnitario
            )).ToList()
        );
    }

    // =========================
    // CREATE (REGRAS APLICADAS)
    // =========================
    public async Task<PedidoDto> CreateAsync(CreatePedidoDto dto)
    {
        // REGRA 1: não permitir pedido sem itens
        if (dto.ItensPedido == null || !dto.ItensPedido.Any())
            throw new Exception("Pedido deve conter pelo menos um item.");

        // REGRA 2: quantidade deve ser > 0
        if (dto.ItensPedido.Any(i => i.Quantidade <= 0))
            throw new Exception("Quantidade deve ser maior que zero.");


        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            ClienteNome = dto.ClienteNome,
            DataCriacao = DateTime.UtcNow,
            Status = StatusPedido.Novo,


            ItensPedido = dto.ItensPedido.Select(i => new ItemPedido
            {
                Id = Guid.NewGuid(),
                NomeProduto = i.NomeProduto,
                Quantidade = i.Quantidade,
                ValorUnitario = i.ValorUnitario
            }).ToList()
        };       

        // REGRA 3: calcular total automaticamente
        pedido.ValorTotal = pedido.ItensPedido
            .Sum(i => i.Quantidade * i.ValorUnitario);

        await _context.Pedidos.AddAsync(pedido);
        await _context.SaveChangesAsync();

        return Map(pedido);
    }

    // =========================
    // UPDATE (REGRAS APLICADAS)
    // =========================
    public async Task<PedidoDto?> UpdateAsync(Guid id, UpdatePedidoDto dto)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.ItensPedido)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null) return null;

        var statusAnterior = pedido.Status;

        // =========================
        // CLIENTE
        // =========================
        if (statusAnterior != StatusPedido.Pago &&
            !string.IsNullOrWhiteSpace(dto.ClienteNome))
        {
            pedido.ClienteNome = dto.ClienteNome;
        }

        // =========================
        // STATUS
        // =========================
        StatusPedido? novoStatus = null;

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (!Enum.TryParse<StatusPedido>(dto.Status, true, out var parsedStatus))
                throw new Exception($"Status inválido: {dto.Status}");

            novoStatus = parsedStatus;
        }

        // =========================
        // REGRA: NÃO ALTERA PEDIDO JÁ PAGO
        // =========================
        if (statusAnterior == StatusPedido.Pago)
        {
            throw new Exception("Pedido pago não pode ser alterado.");
        }

        // =========================
        // TRANSIÇÃO STATUS
        // =========================
        if (novoStatus.HasValue)
        {
            if (novoStatus == StatusPedido.Pago)
            {
                if (pedido.ItensPedido == null || !pedido.ItensPedido.Any())
                    throw new Exception("Pedido não pode ser pago sem itens.");

                pedido.Status = StatusPedido.Pago;
            }
            else
            {
                pedido.Status = novoStatus.Value;
            }
        }

        // =========================
        // ITENS - TRATATIVAS
        // =========================
        if (statusAnterior != StatusPedido.Pago && dto.ItensPedido != null)
        {
            if (!dto.ItensPedido.Any())
                throw new Exception("Pedido deve conter pelo menos um item.");

            if (dto.ItensPedido.Any(i => i.Quantidade <= 0))
                throw new Exception("Quantidade deve ser maior que zero.");

            // FORÇA RESET LIMPO DO GRAFO
            pedido.ItensPedido.Clear();

            foreach (var item in dto.ItensPedido)
            {
                pedido.ItensPedido.Add(new ItemPedido
                {
                    NomeProduto = item.NomeProduto,
                    Quantidade = item.Quantidade,
                    ValorUnitario = item.ValorUnitario,
                    PedidoId = pedido.Id // opcional, mas ajuda consistência
                });
            }
        }

        // =========================
        // TOTAL
        // =========================
        pedido.ValorTotal = pedido.ItensPedido
            .Sum(i => i.Quantidade * i.ValorUnitario);

        await _context.SaveChangesAsync();

        return Map(pedido);
    }

    // =========================
    // DELETE
    // =========================
    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await _context.Pedidos
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync();

        return result > 0;
    }

    // =========================
    // CANCELAR PEDIDO
    // =========================
    public async Task<PedidoDto?> CancelarAsync(Guid id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.ItensPedido)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
            return null;

        // REGRA: não pode cancelar se já pago
        if (pedido.Status == StatusPedido.Pago)
            throw new Exception("Pedido já pago não pode ser cancelado.");

        // REGRA: já cancelado
        if (pedido.Status == StatusPedido.Cancelado)
            throw new Exception("Pedido já está cancelado.");

        pedido.Status = StatusPedido.Cancelado;

        await _context.SaveChangesAsync();

        return Map(pedido);
    }

    // =========================
    // MAPPER
    // =========================
    private static PedidoDto Map(Pedido pedido)
    {
        return new PedidoDto(
            pedido.Id,
            pedido.ClienteNome,
            pedido.DataCriacao,
            pedido.ValorTotal,
            pedido.Status.ToString(),
            pedido.ItensPedido.Select(i => new ItemPedidoDto(
                i.Id,
                i.NomeProduto,
                i.Quantidade,
                i.ValorUnitario
            )).ToList()
        );
    }
}