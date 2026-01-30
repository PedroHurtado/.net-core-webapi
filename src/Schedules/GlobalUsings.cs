#pragma warning disable IDE0005

global using System.Reflection;
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

global using Schedules.Infrastructure;

global using Schedules.Features.Schedules.Domain.ScheduleAggregate.ValueObjects;

global using Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate.Enums;
global using Schedules.Features.ServiceSchedules.Domain.ServiceScheduleAggregate.ValueObjects;
