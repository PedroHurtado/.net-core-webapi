global using FluentAssertions;
global using FluentValidation;
global using Fudie.Domain;
global using Fudie.Infrastructure;
global using Moq;

global using Customer.Features.Menus.Api.Commands.Allergens;
global using Customer.Features.Menus.Api.Queries.Allergens;

global using Customer.Features.Menus.Domain.MenuAggregate.Enums;
global using Customer.Features.Menus.Domain.Shared.Enums;
global using Customer.Features.Menus.Domain.Shared.ValueObjects;
global using Customer.Features.Menus.Domain.MenuItemAggregate.ValueObjects;
global using Customer.Features.Menus.Domain.AllergenAggregate;
global using Customer.Features.Menus.Domain.MenuItemAggregate;
global using Customer.Features.Menus.Domain.MenuAggregate;
global using Customer.Features.Menus.Domain.MenuAggregate.Entities;
global using Customer.Features.Menus.Domain.MenuAggregate.ValueObjects;

global using Customer.UnitTests.Helpers;

global using PriceOptionVO = Customer.Features.Menus.Domain.Shared.ValueObjects.PriceOption;
global using CategoryItemVO = Customer.Features.Menus.Domain.Shared.ValueObjects.CategoryItem;
global using ItemDepositOverrideVO = Customer.Features.Menus.Domain.MenuItemAggregate.ValueObjects.ItemDepositOverride;
global using NutritionalInfoVO = Customer.Features.Menus.Domain.MenuItemAggregate.ValueObjects.NutritionalInfo;
global using DepositPolicyVO = Customer.Features.Menus.Domain.MenuAggregate.ValueObjects.DepositPolicy;
global using MenuAgg = Customer.Features.Menus.Domain.MenuAggregate.Menu;
global using MenuCategoryEntity = Customer.Features.Menus.Domain.MenuAggregate.Entities.MenuCategory;
global using MenuItemAgg = Customer.Features.Menus.Domain.MenuItemAggregate.MenuItem;
global using AllergenAgg = Customer.Features.Menus.Domain.AllergenAggregate.Allergen;