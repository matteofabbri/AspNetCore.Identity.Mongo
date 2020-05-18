## Migration guide to version 6.7.0+
Started from version 6.7.0 library has 2 big changes:
* `MongoRole` and `MongoUser` use native MongoBD ObjectId type instead of string.
* `MongoRole` has new property `Claims`.

According to changes you need to do 2 updates:
1. update **"_id"** from **"5e88749c85924566b02855cd"** to **ObjectId("5e88749c85924566b02855cd")** in Users and Roles collections
2. add empty array to Roles collection

Lower you can find simple scripts to do this updated.<br>
**NOTE!:** Before do anything please **[dump](https://docs.mongodb.com/manual/reference/program/mongodump/index.html)** your data.


1. 
```
db.Users.find({_id : {$type : 2}}). //find all _id prop with type string 
    forEach(function(item){
        var oldId = item._id;
        var id = ObjectId(oldId); //_id to ObjectId  
        item._id = id; 
        db.Users.insert(item);
        db.Users.remove({ _id : oldId });
        }
    )
```
Then run this script for Roles collection. Simple change Users => Roles (3 places)<br>
[MongoDB types](https://docs.mongodb.com/manual/reference/operator/query/type/)
2. 
```
db.Roles.updateMany({}, {$set : {Claims : []}})
```