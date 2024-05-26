using AutoMapper;
using Store.Data.Entities;
using Store.Data.Entities.OrderEntities;
using Store.Repository.BasketRepositories.Models;
using Store.Repository.Interfaces;
using Store.Repository.Specification.Order;
using Store.Service.Services.BasketService;
using Store.Service.Services.OrderService.Dtos;
using Store.Service.Services.PaymentService;

namespace Store.Service.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBasketService _basketService;
        private readonly IMapper _mapper;
        private readonly IPaymentService _paymentService;

        public OrderService(
            IUnitOfWork unitOfWork,
            IBasketService basketService,
            IMapper mapper,
            IPaymentService paymentService)
        {
            _unitOfWork = unitOfWork;
            _basketService = basketService;
            _mapper = mapper;
            _paymentService = paymentService;
        }
        public async Task<OrderResultDto> CreateOrderAsync(OrderDto input)
        {
            // Get Basket
            var basket = await _basketService.GetBasketAsync(input.BasketId);

            if (basket is null)
                throw new Exception("Basket Not Exist");

            // Fill Order Items From Basket Items
            var orderItems = new List<OrderItemDto>();

            foreach (var basketItem in basket.BasketItem)
            {
                var productItem = await _unitOfWork.Repository<Product, int>().GetByIdAsync(basketItem.ProductId);

                if (productItem is null)
                    throw new Exception($"Product With Id : {basketItem.ProductId} Not Exist");

                var itemOrderd = new ProductItemOrdered
                {
                    ProductItemId = productItem.Id,
                    ProductName = productItem.Name,
                    ProductUrl = productItem.PictureUrl
                };

                var orderItem = new OrderItem
                {
                    Price = productItem.Price,
                    Quantity = basketItem.Quantity,
                    ItemOrdered = itemOrderd
                };

                var mappedOrderItem = _mapper.Map<OrderItemDto>(orderItem);

                orderItems.Add(mappedOrderItem);
            }

            // Get Delivery Method
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod, int>().GetByIdAsync(input.DeliveryMethodId);

            if (deliveryMethod is null)
                throw new Exception("deliveryMethod Not Exist");

            // calculate subtotal
            var subtotal = orderItems.Sum(item => item.Quantity * item.Price);

            // TO Do => Check If Order Exist
            var specs = new OrderWithPaymentIntentSpecification(basket.PaymentIntentId);

            var existingOrder = await _unitOfWork.Repository<Order, Guid>().GetWithSpecificationByIdAsync(specs);

            if (existingOrder != null)
            {
                _unitOfWork.Repository<Order, Guid>().Delete(existingOrder);

                await _paymentService.CreateOrUpdatePaymentIntentForExistingOrder(basket);
            }
            else
            {
                await _paymentService.CreateOrUpdatePaymentIntentForNewOrder(basket.Id);

            }

            // Create Order
            var mappedShippingAddress = _mapper.Map<ShippingAddress>(input.ShippingAddress);

            var mappedOrderItems = _mapper.Map<List<OrderItem>>(orderItems);

            var order = new Order
            {
                DeliveryMethodId = deliveryMethod.Id,
                ShippingAddress = mappedShippingAddress,
                BuyerEmail = input.BuyerEmail,
                OrderItems = mappedOrderItems,
                SubTotal = subtotal,
                BasketId = basket.Id,
                PaymentIntentId = basket.PaymentIntentId
            };

            await _unitOfWork.Repository<Order, Guid>().AddAsync(order);

            await _unitOfWork.CompleteAsync();

            var mappedOrder = _mapper.Map<OrderResultDto>(order);

            return mappedOrder;
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetAllDeliveryMethodsAsync()
            => await _unitOfWork.Repository<DeliveryMethod, int>().GetAllAsync();

        public async Task<IReadOnlyList<OrderResultDto>> GetAllOrdersForUserAsync(string buyerEmail)
        {
            var specs = new OrderWithItemsSpecification(buyerEmail);

            var orders = await _unitOfWork.Repository<Order, Guid>().GetWithSpecificationAllAsync(specs);

            if (orders is { Count: <= 0 })
                throw new Exception("You Do Not Have Any Orders Yet");

            var mappedOrders = _mapper.Map<List<OrderResultDto>>(orders);

            return mappedOrders;
        }

        public async Task<OrderResultDto> GetOrderByIdAsync(Guid id, string buyerEmail)
        {
            var specs = new OrderWithItemsSpecification(id, buyerEmail);

            var order = await _unitOfWork.Repository<Order, Guid>().GetWithSpecificationByIdAsync(specs);

            if (order is null)
                throw new Exception($"There Is No Order With Id : {id}");

            var mappedOrder = _mapper.Map<OrderResultDto>(order);

            return mappedOrder;
        }
    }
}
