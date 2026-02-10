global using FluentAssertions;
global using FluentValidation;
global using Moq;

global using Microsoft.Extensions.DependencyInjection;
global using System.Linq.Expressions;

global using Fudie.Domain;
global using Fudie.DependencyInjection;
global using Fudie.Infrastructure;

global using System.Net;
global using System.Security.Cryptography;
global using System.Security.Cryptography.X509Certificates;
global using System.Text.Json;
global using Microsoft.Extensions.Configuration;
global using Microsoft.IdentityModel.JsonWebTokens;
global using Microsoft.IdentityModel.Tokens;

global using Auth.Infrastructure;
global using Auth.Infrastructure.Google;
global using Auth.Infrastructure.Jwt;

global using Auth.Features.Sessions.Api.Queries;

global using Auth.Features.Sessions.Domain.SessionAggregate;

global using Auth.Features.Users.Domain.UserAggregate;
global using Auth.Features.Users.Domain.UserAggregate.Enums;
global using Auth.Features.Users.Domain.UserAggregate.ValueObjects;

global using Auth.UnitTests.Helpers;
