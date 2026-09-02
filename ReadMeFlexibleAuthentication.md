# About Flexible Authentication

##Advantages  
- Easily add new roles and configure access control
- Easily reconfigure access control for existing roles
- Remove roles without impacting existing access control checks
- Easily view access control policies


**References**

- [Github Repo](https://github.com/jasontaylordev/flexible-aspnetcore-authorization/blob/main/Permissions.dib)

- [NDC Talk](https://www.youtube.com/watch?v=TuG0yKf8RSQ&t=11s)

---
---
---
---
---

# 1. About: `GamesGlobal.ShoppingList.WebApi.Identity.AuthorizeAttribute`

### What is `AuthorizeAttribute`?

The `AuthorizeAttribute` is a custom attribute used in this project to secure API endpoints by specifying which permissions a user must have to access them.

### Purpose

- **Access Control:** It restricts access to controllers or actions based on user permissions.
- **Integration:** Extends ASP.NET Core�s built-in `AuthorizeAttribute` to support a strongly-typed `Permissions` enum.

### How It Works

- You can use it in three ways:
  1. **No parameters:** Requires the user to be authenticated.
  2. **With a policy name:** Uses a named policy for authorization.
  3. **With a `Permissions` value:** Uses a specific permission from the `Permissions` enum.

- When you set the `Permissions` property, it automatically generates a policy name using `PermissionPolicyHelper.GeneratePolicyNameFor`. This policy is then checked by the authorization system (see `PermissionAuthorizationHandler` and `PermissionAuthorizationPolicyProvider`).

### Example Usage
```
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Permissions = Permissions.UserRolesAndPermissionsReadOnly)]
```


# 2. About: `GamesGlobal.ShoppingList.WebApi.Identity.PermissionAuthorizationRequirement` & `GamesGlobal.ShoppingList.WebApi.Identity.PermissionAuthorizationHandler`

### Overview

- **`PermissionAuthorizationRequirement`**: Represents a specific permission requirement for accessing a resource. It holds a `Permissions` value indicating what permission(s) are needed.
- **`PermissionAuthorizationHandler`**: Evaluates whether the current user meets the permission requirement specified by `PermissionAuthorizationRequirement`.

### How They Work Together

1. **Requirement Creation**  
   When an endpoint is protected (e.g., via `[Authorize(Permissions.UserRolesAndPermissionsReadOnly)]`), the authorization system creates a `PermissionAuthorizationRequirement` instance with the required `Permissions` value.

2. **Handler Invocation**  
   The ASP.NET Core authorization system calls `PermissionAuthorizationHandler` to evaluate the requirement for the current user.

3. **Permission Check Logic**  
   - The handler receives the `PermissionAuthorizationRequirement` instance as the `requirement` parameter.
   - It extracts the user's permissions from their claims.
   - It compares the user's permissions against `requirement.Permissions` using a bitwise AND operation:

###Example - Register PermissionAuthorizationHandler
`services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();`



# 3. About `GamesGlobal.ShoppingList.WebApi.Identity.PermissionAuthorizationPolicyProvider`?

`PermissionAuthorizationPolicyProvider` is a custom policy provider for ASP.NET Core's authorization system. Its main job is to dynamically create and provide authorization policies based on permission names at runtime.

### Why do we need it?

- **Dynamic Policies:** In many applications, permissions and policies are not always known at compile time. This provider allows the system to generate policies on-the-fly when a request references a permission-based policy name.
- **Integration with Permissions:** It works with the custom `Permissions` enum and the `PermissionAuthorizationRequirement` to enforce permission-based access control.

### How does it work?

1. **Policy Lookup:**  
   When the authorization system needs a policy (e.g., when an endpoint is decorated with `[Authorize("Permission:ShoppingItemsSelfReadWrite")]`), it calls `GetPolicyAsync` with the policy name.

2. **Base Policy Check:**  
   It first checks if the policy already exists using the base provider.

3. **Dynamic Policy Creation:**  
   - If the policy does not exist and the policy name matches a valid permission pattern (`PermissionPolicyHelper.IsValidPolicyName`), it extracts the required permissions from the policy name.
   - It then creates a new `AuthorizationPolicy` with a `PermissionAuthorizationRequirement` for those permissions.
   - The new policy is added to the authorization options for future use.

4. **Return Policy:**  
   The policy is returned to the authorization system, which uses it to enforce access control.

### Example Scenario

- You decorate an endpoint with `[Authorize(Permissions.UserRolesAndPermissionsReadOnly)]`.
- The system asks for a policy named for that permission.
- If the policy doesn't exist, `PermissionAuthorizationPolicyProvider` creates it dynamically.

### Summary

`PermissionAuthorizationPolicyProvider` enables flexible, permission-based authorization by generating policies as needed. This makes it easy to manage and scale permissions without hardcoding every possible policy.


###Example - Register PermissionAuthorizationPolicyProvider
`services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();`

---
---
---
---
---

# Flags

```
[Flags]
public enum Permissions
{
    None = 0,     // 00000  
    A    = 1,     // 00001
    B    = 2,     // 00010
    C    = 4,     // 00100
    D    = 8,     // 01000
    E    = 16,    // 10000
    All  = ~None  // 11111
}
```

 Permissions are stored using a C# flags enum
 Applying the attribute indicates the enum can be treated as a bit field, i.e. a set of flags.
 **Note:** Values are powers of 2, e.g. 2, 4, 8, 16, ... 

None - The operator ~ produces a complement of its operand by reversing each bit,
So ~00000 = 11111 = A, B, C, D, E 

```
var userPermissions = Permissions.A | Permissions.C;     // 00001 + 00100 = 00101 = 5
var requiredPermissions = Permissions.A | Permissions.B; // 00001 + 00010 = 00011 = 3
```

#Printin Flags

1. Output permissions (as string)

```
Console.WriteLine($"User Permissions: {userPermissions}");
Console.WriteLine($"Required Permissions: {requiredPermissions}");
```


2. Output permissions (as int)
```
Console.WriteLine($"User Permissions: {(int)userPermissions}");
Console.WriteLine($"Required Permissions: {(int)requiredPermissions}");
```

#Validate Permissions
Check permissions

```
Console.WriteLine($"User Permissions: {userPermissions}");
Console.WriteLine($"Required Permissions: {requiredPermissions}");
Console.WriteLine($"Authorised: {((requiredPermissions & userPermissions) != 0)}");
```

#Adding Permissions

Add permission (with logical OR operator)
`userPermissions |= Permissions.B;`

Output permissions (as string)
`Console.WriteLine($"User Permissions: {userPermissions}");`

#Removing Permissions
Remove permission (with logical XOR operator)
`userPermissions ^= Permissions.B;`

Output permissions (as string)
`Console.WriteLine($"User Permissions: {userPermissions}");`

#Working with Policies
`requiredPermissions = Permissions.A | Permissions.B;`

Create a policy name
```
[Authorize(Permissions.A | Permission.B)]
private const string PolicyPrefix = "Permissions";
```

`var policyName = $"{PolicyPrefix}{(int)requiredPermissions}";`

// Output policy name
```
Console.WriteLine($"Required Permissions: {requiredPermissions} ({(int)requiredPermissions})");
Console.WriteLine($"Policy Name: '{policyName}'");
```


Update required permissions and policy name
```
requiredPermissions = Permissions.A | Permissions.B | Permissions.C;

policyName = $"{PolicyPrefix}{(int)requiredPermissions}";
```


Output policy name
```
Console.WriteLine($"Required Permissions: {requiredPermissions} ({(int)requiredPermissions})");
Console.WriteLine($"Policy Name: '{policyName}'");
```


Get permissions based on policy name
```
var permissionsValue = int.Parse(policyName[PolicyPrefix.Length..]);


Console.WriteLine($"Policy Name: {policyName}");
Console.WriteLine($"Required Permissions: {(Permissions)permissionsValue}");
```