# Blackbird.io Airtable

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

Airtable is a cloud-based software platform that combines the functionality of a spreadsheet with the power of a database. It allows users to create, organize, and collaborate on tables of data, similar to a spreadsheet, but with added features such as customizable fields, advanced filtering, linking between tables, attachments, and more.

## Before setting up

Before you can connect you need to make sure that:

- You have an Airtable account.
- You have created a base (database) in your Airtable account.

## Connecting

1. Navigate to apps and search for Airtable. If you cannot find Airtable then click _Add App_ in the top right corner, select Airtable and add the app to your Blackbird environment.
2. Click _Add Connection_.
3. Name your connection for future reference e.g. 'My Airtable connection'.
4. Fill in the Base ID to the base you want to connect in your Airtable instance. In Airtable, each database is called a "base," and each base has a unique identifier known as the "base ID." You can find the base ID in the URL of your Airtable base when you are viewing it in the browser. The base ID is the alphanumeric string that appears after "https://airtable.com/" and before "/base".
5. Click Authorize connection.
6. Follow the instructions from Airtable.
7. When you return to Blackbird, confirm that the connection has appeared and the status is Connected.

![AirtableBlackbirdConnection](image/README/AirtableBlackbirdConnection.png)

## Actions

### Records

- **List records** List all records in the table.
- **Search record** Search for one specific record where the field value is compared to the value you input.
- **Add new record** Creates a new record in the table with its primary field filled in. Use the "Update value of field" actions below to fill out the other fields.
- **Delete record** Deletes a record given the record ID.

### Attachments

- **Download files from attachment field** Download files from an attachment field.

### Fields

- **Get value of boolean field** Get the value of a boolean field (e.g. checkbox).
- **Get value of date field** Get the value of a date field.
- **Get value of number field** Get the value of a number field (e.g. number, currency, percent, rating).
- **Get value of string field** Get the value of a string field (e.g. single line text, long text, phone number, email, URL, single select).
- **Update value of boolean field** Update the value of a boolean field (e.g. checkbox).
- **Update value of date field** Update the value of a date field.
- **Update value of number field** Update the value of a number field (e.g. number, currency, percent, rating).
- **Update value of string field** Update the value of a string field (e.g. single line text, long text, phone number, email, URL, single select).

## Events

### Records

- **On records added** This webhook is triggered when new records are added.

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
