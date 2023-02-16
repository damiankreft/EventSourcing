using Marten;
using System;

namespace ShopEventSourcing
{
    public class MartenEventStore
    {
        private readonly IDocumentStore _store;

        public MartenEventStore(string connectionString)
        {
            _store = DocumentStore.For(_ =>
            {
                _.Connection(connectionString);
                _.Schema.For<Order>();
            });
        }

        public Order GetAggregate(Guid id)
        {
            using (var session = _store.QuerySession())
            {
                return session.Events.AggregateStream<Order>(id);
            }
        }

        public void AddEvents(Guid guid)
        {
            OrderPlaced orderPlaced1 = new()
            {
                TotalAmount = 39.99m
            };

            OrderPlaced orderPlaced2 = new()
            {
                TotalAmount = 59.99m
            };

            OrderPlaced orderPlaced3 = new()
            {
                TotalAmount = 19.99m
            };

            OrderPlaced orderPlaced4 = new()
            {
                TotalAmount = 13.49m
            };

            using (var session = _store.OpenSession())
            {
                session.Events.Append(guid, orderPlaced1, orderPlaced2, orderPlaced3, orderPlaced4);
                session.SaveChanges();
            }
        }
    }

    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }

        public void Apply(OrderPlaced @event)
        {
            TotalAmount += @event.TotalAmount;
        }
    }

    public class OrderPlaced
    {
        public int OrderId { get; set; }
        public string OrderItemName { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            var connString = "Host=localhost;Database=shop;Username=postgres;Password=root";
            //var store = DocumentStore.For(connString);
            var store = new MartenEventStore(connString);
            var guid = Guid.NewGuid();
            store.AddEvents(guid);
            var order = store.GetAggregate(guid);
            Console.WriteLine($"Order {guid} for a total amount of {order.TotalAmount:C}pln.");

        }
    }
}
