global using FluentAssertions;
global using FluentValidation;
global using Moq;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Query;
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

global using Auth.Features.ExternalApps.Api.ExternalAppAggregate;
global using Auth.Features.ExternalApps.Api.ExternalAppAggregate.Commands;
global using Auth.Features.ExternalApps.Api.ExternalAppAggregate.Queries;
global using Auth.Features.Memberships.Api.MembershipAggregate;
global using Auth.Features.Memberships.Api.MembershipAggregate.Commands;
global using Auth.Features.Memberships.Api.MembershipAggregate.Queries;
global using Auth.Features.ExternalApps.Domain.ExternalAppAggregate;
global using Auth.Features.Memberships.Domain.MembershipAggregate;
global using Auth.Features.Roles.Domain.TenantRoleAggregate;
global using Auth.Features.Sessions.Domain.SessionAggregate;

global using Auth.Features.Shared.Enums;

global using Auth.Features.Users.Domain.UserAggregate;
global using Auth.Features.Users.Domain.UserAggregate.Enums;
global using Auth.Features.Users.Domain.UserAggregate.ValueObjects;

global using Auth.Features.Roles.Api.TenantRoleAggregate;
global using Auth.Features.Roles.Api.TenantRoleAggregate.Commands;
global using Auth.Features.Roles.Api.TenantRoleAggregate.Queries;
global using Auth.Features.Users.Api.UserAggregate.Commands;

global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Http.HttpResults;

global using Auth.UnitTests.Helpers;

global using Fudie.Tests.Common;
