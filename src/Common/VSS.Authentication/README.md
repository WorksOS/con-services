# VSS.Authentication

Handles server-side authentication concerns like decoding JWTs and authenticating server-to-server API calls.

## Usage

### Decoding a JWT:

```
public class MyApi : ApiController
{
  public MyResponseObject MyMethod()
  {
    var jwt =  new TPaaSJWT(ActionContext.Request.Headers);
    Guid userUid = jwt.UserUid;
    // use the user uid
  }
}
```

You may also have a look under Sample folder - there is a sample for decoding JWT and seeting context principal as a part of filter pipe in MVC 5+ (support for .net46+ and .netstandard1.4+)

### Generating a bearer token:

```
public class MyApi : ApiController
{
  public MyResponseObject MyMethod()
  {
    var session = new Session(
				"https://identity-stg.trimble.com/token",
				"your_application_consumer_key_here",
				"your_application_consumer_secret_here");

			var sessionToken = session.GetToken();
      var bearerToken = sessionToken.AccessToken;
    // use the bearer token in an API call here with whatever http framework you choose
  }
}
```

## Getting Started

### Option 1 - Git Submodule

You can pull in this library using a git submodule:

```
cd YourWebApiProject
git submodule add https://visionlink.visualstudio.com/DefaultCollection/VSS/_git/VSS.Authentication
```

You will now have a copy of the code in a "VSS.Authentication" subfolder in your web api repo. Add a project reference to the desired project:

- `VSS.Authentication.JWT/VSS.Authentication.JWT.csproj` for VS 2017 and .NET Core
  - or `VSS.Authentication.JWT_vs2015.csproj` for VS 2015


*Get Jenkins to clone the submodule*

If you're using free-style jobs, enable the setting "Advanced sub-modules behaviours > Recursively update submodules":
![Submodule config](http://i.imgur.com/p7zQHt7.png)

If you're cloning using Jenkins pipeline groovy code, then right after cloning your repository do the following:
```
bat "git submodule update --init" # for windows slaves
sh "git submodule update --init" # for linux slaves
```

### Option 2 - NuGet

TThere is a Jenkins pipeline at ```https://ci.vspengg.com/view/Nuget%20Packages/job/VSS.Authentication```.

### Option 3 - Copy Paste

Just kidding. This is **NOT** an option. Do not copy paste this code into your repo. This will limit your ability to get updates to this library in the future (which will inevitably happen). Also, if you need to make updates to this codebase, open a Pull Request into this repository.


## Contributing

Run tests:
```
cd VSS.Authentication.JWT.Tests
dotnet restore
dotnet watch test
```

Contributions shall be made through a VSTS pull request with at least 1 tech lead as a reviewer.

### Packaging and publishing to NuGet

Here's how to package and publish new versions to NuGet:

- Pick the appropriate version number using Semantic Versioning 2.0 (semver.org)
- Make sure your working directory is clean
- From PowerShell prompt on Windows run `./package.ps1 -Version X.Y.Z`


*Important note about NuGet and submodules*

When changing NuGet dependencies in this repo, you must do the following workaround for the 2015 csproj files:

- Replace `..\packages` in the `_vs2015.csproj` files with `$(SolutionDir)\packages`

The reason is because NuGet references do not play nicely with solutions in different folders with VS 2015 since the exact relative path to the restore folder is specified by default.
