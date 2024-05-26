using Microsoft.AspNetCore.Mvc;
using Store.Repository.Interfaces;
using Store.Repository.Repositories;
using Store.Service.Services.ProductsService.Dtos;
using Store.Service.Services.ProductsService;
using Store.Service.HandleResponses;
using Store.Service.Services.CacheService;
using Store.Repository.BasketRepositories;
using Store.Service.Services.BasketService.Dtos;
using Store.Service.Services.BasketService;
using Store.Service.Services.TokenService;
using Store.Service.Services.UserService;
using Store.Service.Services.OrderService.Dtos;
using Store.Service.Services.PaymentService;
using Store.Service.Services.OrderService;

namespace Store.API.Extentions
{
    public static class ApplicationServicesExtentions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IBasketRepository, BasketRepository>();
            services.AddScoped<IBasketService, BasketService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddAutoMapper(typeof(ProductProfile));
            services.AddAutoMapper(typeof(BasketProfile));
            services.AddAutoMapper(typeof(OrderProfile));
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errors = actionContext.ModelState
                                        .Where(model => model.Value.Errors.Count > 0)
                                        .SelectMany(model => model.Value.Errors)
                                        .Select(error => error.ErrorMessage)
                                        .ToList();
                    var errorResponse = new ValidationErrorResponse
                    {
                        Errors = errors
                    };
                    return new BadRequestObjectResult(errorResponse);
                };
            });
            return services;
        }
    }
}
