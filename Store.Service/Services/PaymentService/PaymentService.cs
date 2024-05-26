using AutoMapper;
using Microsoft.Extensions.Configuration;
using Store.Data.Entities;
using Store.Data.Entities.OrderEntities;
using Store.Repository.Interfaces;
using Store.Repository.Specification.Order;
using Store.Service.Services.BasketService;
using Store.Service.Services.BasketService.Dtos;
using Store.Service.Services.OrderService.Dtos;
using Stripe;

namespace Store.Service.Services.PaymentService
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IBasketService _basketService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentService(
            IConfiguration configuration,
            IBasketService basketService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _configuration = configuration;
            _basketService = basketService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<CustomerBasketDto> CreateOrUpdatePaymentIntentForExistingOrder(CustomerBasketDto basket)
        {
            StripeConfiguration.ApiKey = _configuration["Stripe:Secretkey"];

            if (basket is null)
                throw new Exception("Basket Is Null");

            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod, int>().GetByIdAsync(basket.DeliveryMethodId.Value);

            var shippingPrice = deliveryMethod.Price;

            foreach (var item in basket.BasketItem)
            {
                var product = await _unitOfWork.Repository<Store.Data.Entities.Product, int>().GetByIdAsync(item.ProductId);

                if (item.Price != product.Price)
                    item.Price = product.Price;
            }

            var service = new PaymentIntentService();

            PaymentIntent paymentIntent;

            if (string.IsNullOrEmpty(basket.PaymentIntentId))
            {
                var option = new PaymentIntentCreateOptions
                {
                    Amount = (long)basket.BasketItem.Sum(item => item.Quantity * (item.Price * 100)) + (long)(shippingPrice * 100),
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> { "card" }
                };

                paymentIntent = await service.CreateAsync(option);

                basket.PaymentIntentId = paymentIntent.Id;
                basket.ClientSecret = paymentIntent.ClientSecret;
            }
            else
            {
                var option = new PaymentIntentUpdateOptions
                {
                    Amount = (long)basket.BasketItem.Sum(item => item.Quantity * (item.Price * 100)) + (long)(shippingPrice * 100)
                };
                await service.UpdateAsync(basket.PaymentIntentId, option);
            }

            await _basketService.UpdateBasketAsync(basket);
            return basket;
        }

        public async Task<CustomerBasketDto> CreateOrUpdatePaymentIntentForNewOrder(string basketId)
        {
            StripeConfiguration.ApiKey = _configuration["Stripe:Secretkey"];

            var basket = await _basketService.GetBasketAsync(basketId);

            if (basket is null)
                throw new Exception("Basket Is Null");

            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod, int>().GetByIdAsync(basket.DeliveryMethodId.Value);

            var shippingPrice = deliveryMethod.Price;

            foreach (var item in basket.BasketItem)
            {
                var product = await _unitOfWork.Repository<Store.Data.Entities.Product, int>().GetByIdAsync(item.ProductId);

                if (item.Price != product.Price)
                    item.Price = product.Price;
            }

            var service = new PaymentIntentService();

            PaymentIntent paymentIntent;

            if (string.IsNullOrEmpty(basket.PaymentIntentId))
            {
                var option = new PaymentIntentCreateOptions
                {
                    Amount = (long)basket.BasketItem.Sum(item => item.Quantity * (item.Price * 100)) + (long)(shippingPrice * 100),
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> { "card" }
                };

                paymentIntent = await service.CreateAsync(option);

                basket.PaymentIntentId = paymentIntent.Id;
                basket.ClientSecret = paymentIntent.ClientSecret;
            }
            else
            {
                var option = new PaymentIntentUpdateOptions
                {
                    Amount = (long)basket.BasketItem.Sum(item => item.Quantity * (item.Price * 100)) + (long)(shippingPrice * 100)
                };
                await service.UpdateAsync(basket.PaymentIntentId, option);
            }

            await _basketService.UpdateBasketAsync(basket);
            return basket;
        }

        public async Task<OrderResultDto> UpdateOrderPaymentFailed(string paymentIntentId)
        {
            var specs = new OrderWithPaymentIntentSpecification(paymentIntentId);

            var order = await _unitOfWork.Repository<Order, Guid>().GetWithSpecificationByIdAsync(specs);

            if (order == null)
                throw new Exception("Order Does Not Exist");

            order.OrderPaymentStatus = OrderPaymentStatus.Failed;

            _unitOfWork.Repository<Order, Guid>().Update(order);

            await _unitOfWork.CompleteAsync();

            var mappedOrder = _mapper.Map<OrderResultDto>(order);

            return mappedOrder;
        }

        public async Task<OrderResultDto> UpdateOrderPaymentSucceeded(string paymentIntentId)
        {
            var specs = new OrderWithPaymentIntentSpecification(paymentIntentId);

            var order = await _unitOfWork.Repository<Order, Guid>().GetWithSpecificationByIdAsync(specs);

            if (order == null)
                throw new Exception("Order Does Not Exist");

            order.OrderPaymentStatus = OrderPaymentStatus.Received;

            _unitOfWork.Repository<Order, Guid>().Update(order);

            await _unitOfWork.CompleteAsync();

            await _basketService.DeleteBasketAsync(order.BasketId);

            var mappedOrder = _mapper.Map<OrderResultDto>(order);

            return mappedOrder;
        }
    }
}
