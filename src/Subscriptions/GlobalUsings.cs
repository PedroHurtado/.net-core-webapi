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

global using Subscriptions.Features.Shared.Domain.Enums;
global using Subscriptions.Features.Shared.Domain.ValueObjects;
global using Subscriptions.Features.Subscriptions.Domain.SubscriptionAggregate;
global using Subscriptions.Features.Subscriptions.Domain.SubscriptionAggregate.Enums;
global using Subscriptions.Infrastructure;
global using Fudie.Security;
