# Notes

Github action failing with permission denied?
```
git update-index --add --chmod=+x ./create-orphan-branch.sh
```

Still failing?  Try running the `./create-orphan-branch.sh` locally once.


Coverage badge failing to be pushed?

Give the workflow the `contents: write` permission.


Coverage showing as 0%

Add the `coverlet.collector` package to your test project and run tests with `dotnet test --collect:"XPlat Code Coverage"`.
