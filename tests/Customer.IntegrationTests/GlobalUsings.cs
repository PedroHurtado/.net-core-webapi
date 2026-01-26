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

global using Customer.IntegrationTests.Fixtures;
