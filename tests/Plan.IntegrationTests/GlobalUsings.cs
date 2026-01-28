global using System.Net;
global using System.Net.Http.Json;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using FluentAssertions;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;

global using Fudie.Firestore.EntityFrameworkCore.Infrastructure;
global using Fudie.Infrastructure;

global using Plans.Features.Plans.Api.PlanAggregate;
global using Plans.Features.Plans.Api.PlanAggregate.Commands;
global using Plans.Features.Plans.Api.PlanAggregate.Queries;
global using Plans.Features.Plans.Domain.PlanAggregate.Enums;
global using Plans.Infrastructure;
global using Plans.IntegrationTests.Fixtures;
