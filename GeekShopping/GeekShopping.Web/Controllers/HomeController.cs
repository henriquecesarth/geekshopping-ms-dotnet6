﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GeekShopping.Web.Controllers;

public class HomeController : Controller
{
    private readonly ICartService _cartService;
    private readonly ILogger<HomeController> _logger;
    private readonly IProductService _productService;

    public HomeController(ILogger<HomeController> logger,
        IProductService productService,
        ICartService cartService)
    {
        _logger = logger;
        _productService = productService;
        _cartService = cartService;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.FindAllProducts("");
        return View(products);
    }

    [Authorize]
    public async Task<IActionResult> Details(int id)
    {
        var token = await HttpContext.GetTokenAsync("access_token");
        var model = await _productService.FindProductById(id, token);
        return View(model);
    }

    [HttpPost]
    [ActionName("Details")]
    [Authorize]
    public async Task<IActionResult> DetailsPost(ProductViewModel model)
    {
        var token = await HttpContext.GetTokenAsync("access_token");

        CartViewModel cart = new()
        {
            CartHeader = new CartHeaderViewModel
            {
                UserId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value
            }
        };

        var cartDetail = new CartDetailViewModel
        {
            Count = model.Count,
            ProductId = model.Id,
            Product = await _productService.FindProductById(model.Id, token)
        };

        var cartDetails = new List<CartDetailViewModel>();
        cartDetails.Add(cartDetail);
        cart.CartDetails = cartDetails;

        var response = await _cartService.AddItemToCart(cart, token);
        if (response != null) return RedirectToAction(nameof(Index));
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Authorize]
    public async Task<IActionResult> Login()
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Logout()
    {
        return SignOut("Cookies", "oidc");
    }
}