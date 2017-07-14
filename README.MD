# Razor Pages CDN

Example ASP.Core Razor Page application which adds a gulp process to push assets to a Azure CDN.

## CDN Image

A custom TagHelper for CDN based Images is used to get the image from the CDN and has a Fallback Image Source if the image is not available.

The TagHelper inherits from the ImageTagHelper and uses the `asp-append-version` attribute for cache busting to ensure the most up to date version of the image is available.

**Source**

```html
<img src="/images/captamerica.jpg"
    asp-cdn-uri="https://shboyer2.azureedge.net"
    asp-append-version="true" alt="Captain America"
    asp-fallback-src="~/images/deadpool.jpg"
/>
```

**HTML Rendered**

```html
<img
  src="https://shboyer2.azureedge.net/images/captamerica.jpg?v=yY-cpsYC7lzzUnWEx7riu6MHWngzWa90Z1x6JzsM_hM"
  alt="Captain America"
  onerror="this.src='/images/deadpool.jpg?v=nMTfG6JvY--KrkqZP7mEuXOz8EpkcYZALf2-QGN5yeU';console.log('/images/captamerica.jpg?v=yY-cpsYC7lzzUnWEx7riu6MHWngzWa90Z1x6JzsM_hM NOT FOUND.')"
>
```

## Publish Process

Pushing the images to the Azure CDN is accomplished through a gulp task.

```javascript
var azure = require('gulp-azure-storage');

gulp.task("pushimages", function() {
    return gulp.src("wwwroot/images/**")
      .pipe(azure.upload({
          account:    process.env.ACCOUNT_NAME,
          key:        process.env.ACCOUNT_KEY,
          container:  process.env.CONTAINER_NAME
      }));
  });
```

The `gulp-azure-storage` npm package requires three parameters:

* ACCOUNT_NAME : This is the name of the Storage Account in Azure
* ACCOUNT_KEY : The Access Key for the Storage Account.
* CONTAINER_NAME : The name of the Container within the Storage Account i.e. "images"

Notice the script accesses the values from `process.env.*`, in my case the values are set via script locally prior to running. *I have a _development.sh and _production.sh file allowing for testing each environment*

```bash
#!/bin/bash
ASPNETCORE_ENVIRONMENT="Development"
ACCOUNT_NAME="my-account-name"
ACCOUNT_KEY="really-long-key-value"
CONTAINER_NAME="images"

echo "ASP.NET Environment:" $ASPNETCORE_ENVIRONMENT
echo "Storage Account: " $ACCOUNT_NAME
echo "Account Key: " $ACCOUNT_KEY
echo "Container Name: " $CONTAINER_NAME
```

For CI/CD processes like VSTS or Travis etc.; set these ENV variables where appropriate in the build scripts.

## Options for running

Pre-build commands to the csproj file which runs the process upon F5 in Visual Studio or `dotnet build` on the command-line.

*The `gulp publish` task calls the `min` and `pushimages` task*

```xml
  <Target Name="MyPreCompileTarget" BeforeTargets="Build">
    <Exec Command="gulp publish" />
  </Target>
```

Command Line

```bash
gulp publish
```

VSTS - Build the pipleline that does the process of executing the scripts and pushes the application to the necessary slot location in Azure.

Running the command yeilds the following output:

```bash
$ gulp publish
Using gulpfile ~/pagescdn/gulpfile.js
Starting 'min:js'...
Starting 'min:css'...
Starting 'min:html'...
Starting 'pushimages'...
Finished 'min:js' after 78 ms
Finished 'min:css' after 94 ms
uploading [======] 100%
6 files uploaded.
```