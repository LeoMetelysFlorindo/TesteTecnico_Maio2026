using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PedioApi.Data;
using PedioApi.DTOs;
using PedioApi.models;
using PedioApi.Services;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Xunit;
using Assert = Xunit.Assert;

namespace TestPedidoApi;

public class PedidoServiceTests
{
    private AppDbContext CreateInMemoryContext()
    {
        // Configure in-memory database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsPedidoDto()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var logger = new NullLogger<PedidoService>();
        var service = new PedidoService(context, logger);

        var dto = new CreatePedidoDto(
            ClienteNome: "Test Cliente",
            ItensPedido: new List<CreateItemPedidoDto>
            {
                new CreateItemPedidoDto("Product A", 2, 50m)
            }
        );

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Cliente", result.ClienteNome);
        Assert.Equal(100m, result.ValorTotal);
        Assert.Single(result.ItensPedido);
        Assert.Equal("Product A", result.ItensPedido[0].NomeProduto);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var logger = new NullLogger<PedidoService>();
        var service = new PedidoService(context, logger);

        // Act
        var result = await service.GetByIdAsync(Guid.Parse("162636fc-2f20-4f10-9bed-0d68ccfd5719"));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingPedido_UpdatesFields()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var pedidoId = Guid.Parse("162636fc-2f20-4f10-9bed-0d68ccfd5719");

        var initialItem = new ItemPedido
        {
            Id = Guid.NewGuid(),
            PedidoId = pedidoId,
            NomeProduto = "Original Product",
            Quantidade = 1,
            ValorUnitario = 50m
        };

        var pedido = new Pedido
        {
            Id = pedidoId,
            ClienteNome = "Original Name",
            ValorTotal = 50.00m,
            Status = StatusPedido.Novo,
            DataCriacao = DateTime.UtcNow,
            ItensPedido = new List<ItemPedido> { initialItem }
        };

        context.ItensPedido.Add(initialItem);
        context.Pedidos.Add(pedido);
        await context.SaveChangesAsync();

        var logger = new NullLogger<PedidoService>();
        var service = new PedidoService(context, logger);

        var updateDto = new UpdatePedidoDto(
            ClienteNome: "Updated Name",
            Status: "Pago",
            ItensPedido: new List<CreateItemPedidoDto>
            {
                new CreateItemPedidoDto("Updated Product", 3, 20m)
            }
        );

        // Act
        var result = await service.UpdateAsync(pedidoId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.ClienteNome);
        Assert.Equal("Pago", result.Status);
        Assert.Equal(60m, result.ValorTotal);
        Assert.Single(result.ItensPedido);
        Assert.Equal("Updated Product", result.ItensPedido[0].NomeProduto);
    }

    [Fact]
    public async Task DeleteAsync_ExistingPedido_ReturnsTrue()
    {
        // Arrange
        using var context = CreateInMemoryContext();       
        var pedidoId = Guid.Parse("162636fc-2f20-4f10-9bed-0d68ccfd5719");

        var initialItem = new ItemPedido
        {
            Id = Guid.NewGuid(),
            PedidoId = pedidoId,
            NomeProduto = "Original Product",
            Quantidade = 1,
            ValorUnitario = 50m
        };

        var pedido = new Pedido
        {
            Id = pedidoId,
            ClienteNome = "Original Name",
            ValorTotal = 50.00m,
            Status = StatusPedido.Novo,
            DataCriacao = DateTime.UtcNow,
            ItensPedido = new List<ItemPedido> { initialItem }
        };

        context.ItensPedido.Add(initialItem);
        context.Pedidos.Add(pedido);
        await context.SaveChangesAsync();

        var logger = new NullLogger<PedidoService>();
        var service = new PedidoService(context, logger);

        // Act
        var result = await service.DeleteAsync(pedidoId);

        // Assert
        Assert.True(result);
        Assert.Null(await context.Pedidos.FindAsync(pedidoId));
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_FiltersResults()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var pedidoId = Guid.Parse("162636fc-2f20-4f10-9bed-0d68ccfd5719");

        var initialItem = new ItemPedido
        {
            Id = Guid.NewGuid(),
            PedidoId = pedidoId,
            NomeProduto = "Original Product",
            Quantidade = 1,
            ValorUnitario = 50m
        };

        var pedido = new Pedido
        {
            Id = pedidoId,
            ClienteNome = "Original Name",
            ValorTotal = 50.00m,
            Status = StatusPedido.Novo,
            DataCriacao = DateTime.UtcNow,
            ItensPedido = new List<ItemPedido> { initialItem }
        };


        context.ItensPedido.Add(initialItem);

        context.Pedidos.AddRange(
            new Pedido
            {
                Id = pedidoId,
                ClienteNome = "Leonardo Metelys",
                ValorTotal = 50.00m,
                Status = StatusPedido.Novo,
                DataCriacao = DateTime.UtcNow,
                ItensPedido = new List<ItemPedido> { initialItem }
            },
             new Pedido
             {
                 Id = pedidoId,
                 ClienteNome = "Marcelo Rodrigues",
                 ValorTotal = 50.00m,
                 Status = StatusPedido.Novo,
                 DataCriacao = DateTime.UtcNow,
                 ItensPedido = new List<ItemPedido> { initialItem }
             },
             new Pedido 
                {
                    Id = pedidoId,
                    ClienteNome = "Karol Santos",
                    ValorTotal = 50.00m,
                    Status = StatusPedido.Novo,
                    DataCriacao = DateTime.UtcNow,
                    ItensPedido = new List<ItemPedido> { initialItem }
                }
        );

        context.ItensPedido.Add(initialItem);
        context.Pedidos.Add(pedido);
        await context.SaveChangesAsync();

        await context.SaveChangesAsync();

        var logger = new NullLogger<PedidoService>();
        var service = new PedidoService(context, logger);

        // Act
        var (items, totalCount) = await service.GetAllAsync(search: "Apple");

        // Assert
        Assert.Equal(2, totalCount);
        Assert.All(items, p => Assert.Contains("Leonardo", p.ClienteNome));
    }

}



