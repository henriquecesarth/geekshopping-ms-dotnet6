﻿using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using GeekShopping.Web.Utils;

namespace GeekShopping.Web.Services;

public class CartService : ICartService
{
    public const string BasePath = "api/v1/cart";
    private readonly HttpClient _client;

    public CartService(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<CartViewModel> FindCartByUserId(string userId, string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync($"{BasePath}/find-cart/{userId}");
        return await response.ReadContentAs<CartViewModel>();
    }

    public async Task<CartViewModel> AddItemToCart(CartViewModel model, string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJson($"{BasePath}/add-cart", model);
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<CartViewModel>();
        throw new Exception("Something went wrong when calling API");
    }

    public async Task<CartViewModel> UpdateCart(CartViewModel model, string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PutAsJson($"{BasePath}/update-cart", model);
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<CartViewModel>();
        throw new Exception("Something went wrong when calling API");
    }

    public async Task<bool> RemoveFromCart(long cartId, string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.DeleteAsync($"{BasePath}/remove-cart/{cartId}");
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<bool>();
        throw new Exception("Something went wrong when calling API");
    }

    public async Task<bool> ApplyCoupon(CartViewModel model, string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJson($"{BasePath}/apply-coupon", model);
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<bool>();
        throw new Exception("Something went wrong when calling API");
    }

    public async Task<bool> RemoveCoupon(string userId, string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.DeleteAsync($"{BasePath}/remove-coupon/{userId}");
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<bool>();
        throw new Exception("Something went wrong when calling API");
    }

    public async Task<object> Checkout(CartHeaderViewModel model, string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJson($"{BasePath}/checkout", model);
        
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<CartHeaderViewModel>();
        else if (response.StatusCode == HttpStatusCode.PreconditionFailed)
            return "Coupon price has changed, please confirm!";
        
        
        throw new Exception("Something went wrong when calling API");
    }

    public async Task<bool> ClearCart(string userId, string token)
    {
        throw new NotImplementedException();
    }
}