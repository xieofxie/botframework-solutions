Please refer to the [Virtual Assistant Template documentation](http://aka.ms/virtualassistantdocs) for deployment and customization instructions.

In General

Switch name should start with # to be distinguished from slots.

In Skill

* Add MockWhitelistAuthenticationProvider with VA appId in AppsWhitelist
* Add "switches": [ { "id": "switch name" } ] in manifestTemplate.json
* Check permission like
```
protected bool PermissionExist(ITurnContext turnContext, string permission)
{
    var v = turnContext?.Activity?.SemanticAction?.Entities?.ContainsKey(permission);
    return v == null ? false : (bool)v;
}
```

In VA

* Need Directory.Read.All in Service Provider Connection Setting
* Add dummy group permission in appsettings.json like
```
"groupPermissions": [
        {
            "id": "group id",
            "skillPermissions": [
                {
                    "id": "skill id",
                    "permissions": [
                        "switch name"
                    ]
                }
            ]
        }
    ]
```
