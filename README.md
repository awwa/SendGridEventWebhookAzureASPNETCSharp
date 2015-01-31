This code is not production ready. It is meant to help you integrate quickly with SendGrid when developing for ASP.NET/Microsoft Azure. You will need to incorporate error handling and testing. 

## What is this?

This code receives SendGrid Event Webhook POST. This stores them to Azure DocumentDB. 

## Setup

- You need to create DocumentDB account, database and collection in advance.
- Edit appSettings tag in Web.config file. See Web.Release.config.
- Deploy this application on Azure Websites.
- Setup SendGrid Event Webhook URL.

Please let me know how I can improve this tutorial with a pull request or open an issue. Thanks! 
