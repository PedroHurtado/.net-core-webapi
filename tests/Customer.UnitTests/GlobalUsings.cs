global using FluentAssertions;
global using FluentValidation;

global using Customer.Features.Menus.Domain.MenuAggregate.Enums;
global using Customer.Features.Menus.Domain.Shared.Enums;
global using Customer.Features.Menus.Domain.Shared.ValueObjects;
global using Customer.Features.Menus.Domain.MenuItemAggregate.ValueObjects;
global using Customer.Features.Menus.Domain.AllergenAggregate;
global using Customer.Features.Menus.Domain.MenuItemAggregate;

global using Customer.UnitTests.Helpers;

global using PriceOptionVO = Customer.Features.Menus.Domain.Shared.ValueObjects.PriceOption;
global using ItemDepositOverrideVO = Customer.Features.Menus.Domain.MenuItemAggregate.ValueObjects.ItemDepositOverride;
global using NutritionalInfoVO = Customer.Features.Menus.Domain.MenuItemAggregate.ValueObjects.NutritionalInfo;