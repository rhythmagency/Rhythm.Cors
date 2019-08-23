# Rhythm.Cors ![GitHub](https://img.shields.io/github/license/rhythmagency/Rhythm.Cors)

A library for .NET websites which provides configurable site-wide CORS request handling.

## Background

Browsers normally prevent webpages from dynamically referencing resources from domains other than their own. This is a security
feature, and is usually not a problem. However, when you do need to go around this policy, the system for doing so is called CORS.
CORS (which stands for "Cross-Origin Resource Sharing") allows you to specify certain domains which are permitted to reference
resources from your server. This library implements that system, and provides a simple way to configure it for .NET websites.

For further information about CORS, read [here](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS).

## Installation
Use NuGet to install this package.

## Usage

On installation, this pacakge adds a section to your web.config, like so:

```xml
<corsConfiguration>
  <rules>
    <!--<add domain="example.com" policy="ALLOW" />-->
  </rules>
</corsConfiguration>
```
  
Create a rule in this section for each external domain you want to allow access, and save the file. Changes are reflected
immediately and automatically for every url on the site.
