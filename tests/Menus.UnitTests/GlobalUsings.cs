global using System.Linq.Expressions;
global using FluentAssertions;
global using FluentValidation;
global using Fudie.Domain;
global using Fudie.Infrastructure;
global using Moq;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Infrastructure;
global using Microsoft.EntityFrameworkCore.Metadata;

global using Menus.Infrastructure;
global using Fudie.Firestore.EntityFrameworkCore.Infrastructure;

global using Menus.Features.Menus.Api.AllergenAggregate.Commands;
global using Menus.Features.Menus.Api.AllergenAggregate.Queries;
global using Menus.Features.Menus.Api.MenuAggregate;
global using Menus.Features.Menus.Api.MenuAggregate.Commands;
global using Menus.Features.Menus.Api.MenuAggregate.Queries;
global using Menus.Features.Menus.Api.MenuItemAggregate;
global using Menus.Features.Menus.Api.MenuItemAggregate.Commands;
global using Menus.Features.Menus.Api.MenuItemAggregate.Queries;

global using Menus.Features.Menus.Domain.MenuAggregate.Enums;
global using Menus.Features.Menus.Domain.Shared.Enums;
global using Menus.Features.Menus.Domain.Shared.ValueObjects;
global using Menus.Features.Menus.Domain.MenuItemAggregate.ValueObjects;
global using Menus.Features.Menus.Domain.AllergenAggregate;
global using Menus.Features.Menus.Domain.MenuItemAggregate;
global using Menus.Features.Menus.Domain.MenuAggregate;
global using Menus.Features.Menus.Domain.MenuAggregate.Entities;
global using Menus.Features.Menus.Domain.MenuAggregate.ValueObjects;

global using Menus.UnitTests.Helpers;

global using PriceOptionVO = Menus.Features.Menus.Domain.Shared.ValueObjects.PriceOption;
global using CategoryItemVO = Menus.Features.Menus.Domain.Shared.ValueObjects.CategoryItem;
global using ItemDepositOverrideVO = Menus.Features.Menus.Domain.MenuItemAggregate.ValueObjects.ItemDepositOverride;
global using NutritionalInfoVO = Menus.Features.Menus.Domain.MenuItemAggregate.ValueObjects.NutritionalInfo;
global using DepositPolicyVO = Menus.Features.Menus.Domain.MenuAggregate.ValueObjects.DepositPolicy;
global using MenuAgg = Menus.Features.Menus.Domain.MenuAggregate.Menu;
global using MenuCategoryEntity = Menus.Features.Menus.Domain.MenuAggregate.Entities.MenuCategory;
global using MenuItemAgg = Menus.Features.Menus.Domain.MenuItemAggregate.MenuItem;
global using AllergenAgg = Menus.Features.Menus.Domain.AllergenAggregate.Allergen;