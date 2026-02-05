global using System.Net;
global using System.Net.Http.Json;
global using FluentAssertions;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;

global using Fudie.Firestore.EntityFrameworkCore.Infrastructure;
global using Fudie.Infrastructure;

global using Menus.Infrastructure;
global using Menus.Features.Menus.Api.AllergenAggregate.Commands;
global using Menus.Features.Menus.Api.AllergenAggregate.Queries;
global using Menus.Features.Menus.Api.MenuItemAggregate;
global using Menus.Features.Menus.Api.MenuItemAggregate.Commands;
global using Menus.Features.Menus.Api.MenuAggregate;
global using Menus.Features.Menus.Api.MenuAggregate.Commands;
global using Menus.Features.Menus.Api.MenuAggregate.Queries;
global using Menus.Features.Menus.Domain.MenuItemAggregate;
global using Menus.Features.Menus.Domain.MenuAggregate;
global using Menus.Features.Menus.Domain.MenuAggregate.Enums;
global using Menus.Features.Menus.Domain.Shared.Enums;

global using Menus.IntegrationTests.Fixtures;

global using MenuItem = Menus.Features.Menus.Domain.MenuItemAggregate.MenuItem;
