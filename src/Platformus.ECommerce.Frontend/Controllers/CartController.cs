﻿// Copyright © 2021 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Magicalizer.Data.Repositories.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Platformus.ECommerce.Data.Entities;
using Platformus.ECommerce.Extensions;
using Platformus.ECommerce.Filters;

namespace Platformus.ECommerce.Frontend.Controllers
{
  public class CartController : Core.Frontend.Controllers.ControllerBase
  {
    private const string CartId = "CartId";

    private IRepository<int, Product, ProductFilter> ProductRepository
    {
      get => this.Storage.GetRepository<int, Product, ProductFilter>();
    }

    private IRepository<int, Position, PositionFilter> PositionRepository
    {
      get => this.Storage.GetRepository<int, Position, PositionFilter>();
    }

    private IRepository<int, Cart, CartFilter> CartRepository
    {
      get => this.Storage.GetRepository<int, Cart, CartFilter>();
    }

    public CartController(IStorage storage) : base(storage)
    {
    }

    [HttpPost]
    public async Task<IActionResult> AddToCartAsync(int productId)
    {
      Cart cart = await this.GetOrCreateCartAsync();
      Product product = await this.ProductRepository.GetByIdAsync(productId);
      Position position = new Position();

      position.CartId = cart.Id;
      position.ProductId = productId;
      position.Price = product.Price;
      position.Quantity = 1m;
      position.Subtotal = position.GetSubtotal();
      this.PositionRepository.Create(position);
      await this.Storage.SaveAsync();
      return this.Ok();
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromCartAsync(int positionId)
    {
      if (this.HttpContext.GetCartManager().TryGetClientSideId(out Guid clientSideId))
      {
        if (await this.PositionRepository.CountAsync(new PositionFilter() { Id = positionId, Cart = new CartFilter() { ClientSideId = clientSideId } }) != 0)
        {
          this.PositionRepository.Delete(positionId);
          await this.Storage.SaveAsync();

          if (await this.PositionRepository.CountAsync(new PositionFilter() { Cart = new CartFilter() { ClientSideId = clientSideId } }) == 0)
          {
            Cart cart = (await this.CartRepository.GetAllAsync(new CartFilter() { ClientSideId = clientSideId })).FirstOrDefault();

            this.CartRepository.Delete(cart.Id);
            await this.Storage.SaveAsync();
            this.Response.Cookies.Delete(CartId);
          }

          return this.Ok();
        }
      }

      return this.Forbid();
    }

    private async Task<Cart> GetOrCreateCartAsync()
    {
      if (this.HttpContext.GetCartManager().TryGetClientSideId(out Guid clientSideId))
      {
        Cart cart = (await this.CartRepository.GetAllAsync(new CartFilter() { ClientSideId = clientSideId })).FirstOrDefault();

        if (cart != null)
          return cart;
      }

      {
        Cart cart = new Cart();

        cart.ClientSideId = Guid.NewGuid();
        cart.Created = DateTime.Now;
        this.CartRepository.Create(cart);
        await this.Storage.SaveAsync();
        this.Response.Cookies.Append(CartId, cart.ClientSideId.ToString());
        return cart;
      }
    }
  }
}