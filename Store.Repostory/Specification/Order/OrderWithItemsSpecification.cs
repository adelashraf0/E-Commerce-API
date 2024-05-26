namespace Store.Repository.Specification.Order
{
    public class OrderWithItemsSpecification : BaseSpecification<Data.Entities.OrderEntities.Order>
    {
        public OrderWithItemsSpecification(string buyerEmail) 
            : base(order => order.BuyerEmail == buyerEmail)
        {
            AddInclude(order => order.OrderItems);
            AddInclude(order => order.DeliveryMethod);
            AddOrderByDescending(order => order.OrderDate);
        }

        public OrderWithItemsSpecification(Guid id, string buyerEmail)
           : base(order => order.BuyerEmail == buyerEmail && order.Id == id)
        {
            AddInclude(order => order.OrderItems);
            AddInclude(order => order.DeliveryMethod);
        }
    }
}
