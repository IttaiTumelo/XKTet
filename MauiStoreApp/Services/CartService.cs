﻿using MauiStoreApp.Models;

namespace MauiStoreApp.Services
{
    public class CartService : BaseService
    {
        private readonly ProductService _productService;
        private List<CartItemDetail> _cartItems = new List<CartItemDetail>(); // Internal cart items collection
        private int? cartId = null;

        public CartService(ProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Retrieves a specific cart based on the provided cart ID.
        /// </summary>
        /// <param name="cartId">The ID of the cart to be retrieved.</param>
        /// <returns>The cart corresponding to the provided cart ID.</returns>
        public async Task<Cart> GetCartAsync(int cartId)
        {
            return await GetAsync<Cart>($"carts/{cartId}");
        }

        /// <summary>
        /// Retrieves the current cart items from the service's local cache.
        /// </summary>
        /// <returns>A list of cart items.</returns>
        public List<CartItemDetail> GetCartItems()
        {
            return _cartItems;
        }

        /// <summary>
        /// Refreshes the cart items based on a specific user's ID.
        /// </summary>
        /// <param name="userId">The ID of the user whose cart items are to be refreshed.</param>
        /// <returns>A task that represents the asynchronous refresh operation.</returns>
        public async Task RefreshCartItemsByUserIdAsync(int userId)
        {
            var carts = await GetAsync<List<Cart>>($"carts/user/{userId}");
            var cart = carts?.FirstOrDefault();

            cartId = cart?.Id;

            if (cart != null)
            {
                _cartItems.Clear();

                foreach (var cartProduct in cart.Products)
                {
                    var productDetails = await _productService.GetProductByIdAsync(cartProduct.ProductId);
                    _cartItems.Add(new CartItemDetail
                    {
                        Product = productDetails,
                        Quantity = cartProduct.Quantity
                    });
                }
            }
        }

        /// <summary>
        /// Deletes the current cart and clears local cart items if the operation is successful.
        /// </summary>
        /// <returns>The HTTP response indicating the result of the delete operation.</returns>
        public async Task<HttpResponseMessage> DeleteCartAsync()
        {
            var response = await DeleteAsync($"carts/{cartId}");
            if (response.IsSuccessStatusCode)
            {
                _cartItems.Clear();
            }
            return response;
        }

        /// <summary>
        /// Adds a product to the current cart or updates its quantity if it already exists in the cart.
        /// </summary>
        /// <param name="product">The instance of the product to be added to the cart.</param>
        public void AddProductToCart(Product product)
        {
            var existingCartItem = _cartItems.FirstOrDefault(item => item.Product.Id == product.Id);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity++;
            }
            else
            {
                _cartItems.Add(new CartItemDetail
                {
                    Product = product,
                    Quantity = 1
                });
            }
        }
    }
}
