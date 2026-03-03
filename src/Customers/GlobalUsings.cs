#pragma warning disable IDE0005

global using System.ComponentModel;
global using System.Globalization;
global using System.Reflection;
global using System.Text.Json;
global using System.Text.Json.Serialization;

global using FluentValidation;
global using Microsoft.AspNetCore.StaticFiles;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.FileProviders;

global using Fudie.Firestore.EntityFrameworkCore.Infrastructure;
global using Fudie.Firestore.EntityFrameworkCore.Metadata.Builders;

global using Customers.Infrastructure;
global using Customers.Features.Customers.Api.CustomerAggregate;
global using Customers.Features.Customers.Domain.CustomerAggregate;
global using Customers.Features.Customers.Domain.CustomerAggregate.ValueObjects;
