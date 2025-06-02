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

1. Navigate to Apps, and identify the **Airtable** app. You can use search to find it.
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
- **Get value of text field** Get the value of a string field (e.g. single line text, long text, phone number, email, URL, single select).
- **Update value of boolean field** Update the value of a boolean field (e.g. checkbox).
- **Update value of date field** Update the value of a date field.
- **Update value of number field** Update the value of a number field (e.g. number, currency, percent, rating).
- **Update value of text field** Update the value of a text field (e.g. long text, phone number, email, URL).
- **Update value of boolean field** Update the value of a single select field.

## Events

- **On data changed** This event is triggered when any data is added/updated/removed (depends on input parameters the user chooses).
- **On cells updated** triggers when certain cells specifically update to a new value. This value can be specified.
- **On select updated** same as the above, except that it provides a useful dropdown for possible values of the select field.

We added extra logic on top of existing Airtable webhooks to avoid duplication of events. But still there is possibility of duplicated events.
According to Airtable documentation: 
`Notification pings are at-least-once. That is, we guarantee that for every change, one notification will be generated, but spurious notifications (when there are no new changes) are also possible.`
[Airtable documentation](https://airtable.com/developers/web/api/webhooks-overview)

Important: Airtable allows only 2 webhooks for one oauth connection at the same time. This means that for one oauth connection in Blackbird you can create only 2 birds with webhooks with different input parameters. If you will try to create third bird with different inputs in Airtable webhook you will get an error during publishing. 

## Example 

Here is an example of how you can use the Airtable app in a workflow:

![example](image/README/example.png)

In this example, the workflow starts with the **On data changed** event, which triggers when any data added to specific table in Airtable. The input paramaters are: Data type - "Table data", Change type - "On add". Then, the workflow uses the **Get value of text field** action to get text content of field we need. In the next step we translate this text via `DeepL` and then send the translated text to Slack channel.

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
