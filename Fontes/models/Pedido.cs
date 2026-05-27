namespace PedioApi.models
{
    public class Pedido
    {        
            public Guid Id { get; set; }
            public string ClienteNome { get; set; }
            public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
            public StatusPedido Status { get; set; }
            public decimal ValorTotal { get; set; }

        public ICollection<ItemPedido> ItensPedido { get; set; }
    }

    public enum StatusPedido
    {
        Novo,
        Pago,
        Cancelado
    }
}
