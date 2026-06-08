using BT.Api.Common.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;

namespace BT.Tests.Architecture;

public sealed class ApiAuthorizationTests
{
    private static readonly string[] ControllersAllowedWithoutPermissionRequirement =
    [
        "AuthController",
        "TotpController"
    ];

    private static readonly string[] ActionsAllowedWithoutPermissionRequirement =
    [
        "IamAdminController.GetNavigationMenus"
    ];

    [Fact]
    public void Api_Controller_Actions_Should_Declare_Authorization_Intent()
    {
        var unsecuredActions = GetControllerActions()
            .Where(action => !HasAuthorizationIntent(action.Controller, action.Method))
            .Select(action => $"{action.Controller.Name}.{action.Method.Name}")
            .Order(StringComparer.Ordinal)
            .ToList();

        unsecuredActions.Should().BeEmpty(
            because: "every API action must explicitly declare [Authorize], [AllowAnonymous], or a permission policy. Found: {0}",
            string.Join(", ", unsecuredActions));
    }

    [Fact]
    public void Feature_Controller_Actions_Should_Use_Permission_Requirements()
    {
        var actionsWithoutPermissions = GetControllerActions()
            .Where(action => !ControllersAllowedWithoutPermissionRequirement.Contains(action.Controller.Name, StringComparer.Ordinal))
            .Where(action => !ActionsAllowedWithoutPermissionRequirement.Contains($"{action.Controller.Name}.{action.Method.Name}", StringComparer.Ordinal))
            .Where(action => !HasPermissionRequirement(action.Controller, action.Method))
            .Select(action => $"{action.Controller.Name}.{action.Method.Name}")
            .Order(StringComparer.Ordinal)
            .ToList();

        actionsWithoutPermissions.Should().BeEmpty(
            because: "feature/admin API actions must use [RequirePermission] so authorization stays permission-driven. Found: {0}",
            string.Join(", ", actionsWithoutPermissions));
    }

    private static IEnumerable<(Type Controller, MethodInfo Method)> GetControllerActions()
    {
        return AssemblyReferences.Api
            .GetTypes()
            .Where(static type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract)
            .Where(static type => type.Namespace?.StartsWith("BT.Api.Features.", StringComparison.Ordinal) == true)
            .SelectMany(static controller => controller
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(HasHttpMethodAttribute)
                .Select(method => (controller, method)));
    }

    private static bool HasHttpMethodAttribute(MethodInfo method)
    {
        return method.GetCustomAttributes(inherit: true).OfType<HttpMethodAttribute>().Any();
    }

    private static bool HasAuthorizationIntent(Type controller, MethodInfo method)
    {
        return HasAttribute<AllowAnonymousAttribute>(controller, method)
            || HasAttribute<AuthorizeAttribute>(controller, method);
    }

    private static bool HasPermissionRequirement(Type controller, MethodInfo method)
    {
        return HasAttribute<RequirePermissionAttribute>(controller, method);
    }

    private static bool HasAttribute<TAttribute>(Type controller, MethodInfo method)
        where TAttribute : Attribute
    {
        return controller.GetCustomAttributes(inherit: true).OfType<TAttribute>().Any()
            || method.GetCustomAttributes(inherit: true).OfType<TAttribute>().Any();
    }
}
