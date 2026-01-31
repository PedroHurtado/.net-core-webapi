#pragma warning disable IDE0005

global using Customer.Features.Menus.Domain.Shared.Enums;
global using Customer.Features.Menus.Domain.Shared.ValueObjects;

global using Customer.Features.Menus.Domain.MenuAggregate;
global using Customer.Features.Menus.Domain.MenuAggregate.Enums;
global using Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;
global using Customer.Features.Menus.Domain.MenuAggregate.Entities;

global using Customer.Features.Menus.Domain.MenuItemAggregate;
global using Customer.Features.Menus.Domain.MenuItemAggregate.ValueObjects;

global using Customer.Features.Menus.Domain.AllergenAggregate;

global using Customer.Features.Menus.Api.MenuItemAggregate;

global using FluentValidation;
global using Fudie.Domain;
global using Fudie.DependencyInjection;
global using Fudie.Features;
global using Fudie.Infrastructure;
global using Fudie.Validation;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;

global using Fudie.Firestore.EntityFrameworkCore.Metadata.Builders;
