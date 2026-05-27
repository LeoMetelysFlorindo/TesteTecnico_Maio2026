namespace PedioApi.DTOs;

public record CreateItemPedidoDto(
    string NomeProduto,
    int Quantidade,
    decimal ValorUnitario
);

// DTO for order creation
public record CreatePedidoDto(
    string ClienteNome,
    List<CreateItemPedidoDto> ItensPedido
);

// DTO for order update
public record UpdatePedidoDto(
    string? ClienteNome,
    string? Status,
    List<CreateItemPedidoDto>? ItensPedido
);

// DTO for item response
public record ItemPedidoDto(
    Guid Id,
    string NomeProduto,
    int Quantidade,
    decimal ValorUnitario
);

// DTO for response (read)
public record PedidoDto(
    Guid Id,
    string ClienteNome,
    DateTime DataCriacao,
    decimal ValorTotal,
    string Status,
    List<ItemPedidoDto> ItensPedido
);

// DTO for list with pagination
public record PedidoListDto(
    Guid Id,
    string ClienteNome,
    DateTime DataCriacao,
    decimal ValorTotal,
    string Status,
    int QuantidadeItens
);

public record PedidoListWithItemsDto(
    Guid Id,
    string ClienteNome,
    DateTime DataCriacao,
    decimal ValorTotal,
    string Status,
    int QuantidadeItens,
    List<ItemPedidoDto> ItensPedido
);