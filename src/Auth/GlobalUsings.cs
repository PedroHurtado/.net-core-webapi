#pragma warning disable IDE0005

global using System.ComponentModel;
global using System.Globalization;
global using System.Reflection;
global using System.Text.Json;
global using System.Text.Json.Serialization;

global using FluentValidation;
global using Fudie.Domain;
global using Fudie.DependencyInjection;
global using Fudie.Features;
global using Fudie.Http;
global using Fudie.Infrastructure;
global using Fudie.Validation;
global using Microsoft.AspNetCore.StaticFiles;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.FileProviders;

global using Fudie.Firestore.EntityFrameworkCore.Infrastructure;
global using Fudie.Firestore.EntityFrameworkCore.Metadata.Builders;

global using Auth.Infrastructure;
global using Auth.Infrastructure.Google;
global using Auth.Infrastructure.Jwt;

global using System.Security.Cryptography;
global using System.Security.Cryptography.X509Certificates;
global using System.Text;
global using Microsoft.AspNetCore.WebUtilities;
global using Microsoft.IdentityModel.JsonWebTokens;
global using Microsoft.IdentityModel.Tokens;
global using Refit;

global using Auth.Features.Roles.Api.TenantRoleAggregate;
global using Auth.Features.Memberships.Domain.MembershipAggregate;
global using Auth.Features.Roles.Domain.TenantRoleAggregate;
global using Auth.Features.Sessions.Domain.SessionAggregate;

global using Auth.Features.Shared.Enums;

global using Auth.Features.Users.Domain.UserAggregate;
global using Auth.Features.Users.Domain.UserAggregate.Enums;
global using Auth.Features.Users.Domain.UserAggregate.ValueObjects;
