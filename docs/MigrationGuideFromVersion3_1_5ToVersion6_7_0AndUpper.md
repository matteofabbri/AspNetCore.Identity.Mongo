## Migration guide from version 3.1.5 to version 6.7.x
Differences between versions:
* `MongoUser.Roles` property contains id of role instead of NormalizedName
* `MongoUser.Claims` has new structure of `Claim` object
* `MongoRole` has 2 new properties `Claims` and `ConcurrencyStamp`.

Lower you can find simple scripts to do necessary updates.<br>
**NOTE!:** Before do anything please **[dump](https://docs.mongodb.com/manual/reference/program/mongodump/index.html)** your data.

1. Users collections:
```
db.Users.updateMany( {}, { $rename: { "LockoutEndDateUtc": "LockoutEnd" } } )
db.Users.find({}).
    forEach(function(item){
        // claims processing
        var newClaimsArray = [];
        item.Claims.forEach(function(claim){
           newClaimsArray.push({
                "_id" : 0,
                "UserId" : null,
                "ClaimType" : claim.Type,
                "ClaimValue" : claim.Value
            }); 
        });
        item.Claims = newClaimsArray;
        
        // roles processing
        var newRolesArray = [];
        item.Roles.forEach(function(role){
           var originalRole = db.Roles.findOne({NormalizedName : role});
           newRolesArray.push(originalRole._id.str); 
        });
        item.Roles = newRolesArray;
        
        db.Users.save(item);
        }
    )
```

2. Roles collcetions:
```
db.Roles.updateMany({}, {$set : {Claims : [], ConcurrencyStamp : ""}})
```