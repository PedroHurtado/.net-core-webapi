global using System.Net;
global using System.Net.Http.Json;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using FluentAssertions;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;

global using Fudie.Infrastructure;

global using Plan.Features.Plans.Api.PlanAggregate;
global using Plan.Features.Plans.Domain.PlanAggregate.Enums;
global using Plan.Infrastructure;
global using Plan.IntegrationTests.Fixtures;
