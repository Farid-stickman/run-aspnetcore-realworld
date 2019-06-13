﻿using System;
using System.Threading.Tasks;
using AspnetRun.Application.Interfaces;
using AspnetRun.Application.Mapper;
using AspnetRun.Application.Models;
using AspnetRun.Core.Entities;
using AspnetRun.Core.Interfaces;
using AspnetRun.Core.Repositories;

namespace AspnetRun.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IAppLogger<CartService> _logger;

        public CartService(ICartRepository cartRepository, IProductRepository productRepository, IAppLogger<CartService> logger)
        {
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CartModel> GetCartByUserName(string userName)
        {
            var cart = await GetExistingOrCreateNewCart(userName);
            var cartModel = ObjectMapper.Mapper.Map<CartModel>(cart);

            foreach (var item in cart.Items)
            {
                var product = await _productRepository.GetProductByIdWithCategoryAsync(item.ProductId);
                var productModel = ObjectMapper.Mapper.Map<ProductModel>(product);
                cartModel.Items.Add(productModel);
            }
            return cartModel;
        }

        public async Task AddItem(string userName, int productId)
        {
            var cart = await GetExistingOrCreateNewCart(userName);
            cart.AddItem(productId);
            await _cartRepository.UpdateAsync(cart);
        }

        public async Task RemoveItem(int compareId, int productId)
        {
            var spec = new CartWithItemsSpecification(CartId);
            var cart = (await _compareRepository.GetAsync(spec)).FirstOrDefault();
            cart.RemoveItem(productId);
            await _cartRepository.UpdateAsync(cart);
        }

        private async Task<Cart> GetExistingOrCreateNewCart(string userName)
        {
            var cart = await _cartRepository.GetByUserNameAsync(userName);
            if (cart != null)
                return cart;

            // if it is first attempt create new
            var newCart = new Cart
            {
                UserName = userName
            };

            await _cartRepository.AddAsync(newCart);
            return newCart;
        }
    }
}