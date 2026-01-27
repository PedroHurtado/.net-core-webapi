global using System.Net;
global using System.Net.Http.Json;
global using FluentAssertions;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;

global using Fudie.Firestore.EntityFrameworkCore.Infrastructure;
global using Fudie.Infrastructure;

global using Customer.Infrastructure;
global using Customer.Features.Menus.Api.AllergenAggregate.Commands;
global using Customer.Features.Menus.Api.AllergenAggregate.Queries;
global using Customer.Features.Menus.Api.MenuItemAggregate;
global using Customer.Features.Menus.Api.MenuItemAggregate.Commands;
global using Customer.Features.Menus.Domain.MenuItemAggregate;
global using Customer.Features.Menus.Domain.Shared.Enums;

global using Customer.IntegrationTests.Fixtures;

global using MenuItem = Customer.Features.Menus.Domain.MenuItemAggregate.MenuItem;
