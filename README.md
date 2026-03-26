# Book Management Console App

This project is a C# console application for managing a small library-style database in SQL Server. It lets a user manage:

- books
- members
- borrow history

All application logic currently lives in [Program.cs](c:\Users\Rezio\Desktop\AUBSoftwareIntern\InternshipPractice3\book_Managment\book_Managment\Program.cs).

## What The Program Does

When the app starts, it:

1. connects to the `book_management` SQL Server database
2. confirms the connection is open
3. enters a console menu loop

From the main dashboard, the user can open three areas:

- `1. Book's Data`
- `2. Member's Data`
- `3. Book's History`

Each area supports basic CRUD operations:

- View
- Add
- Update
- Delete

## Project Structure

- [Program.cs](c:\Users\Rezio\Desktop\AUBSoftwareIntern\InternshipPractice3\book_Managment\book_Managment\Program.cs): all menu logic, validation, and database access
- [book_Managment.csproj](c:\Users\Rezio\Desktop\AUBSoftwareIntern\InternshipPractice3\book_Managment\book_Managment\book_Managment.csproj): .NET project file

## Tech Stack

- .NET `10.0`
- `Microsoft.Data.SqlClient` package version `7.0.0`
- SQL Server with Windows authentication

## Database Connection

The application uses a hard-coded connection string in [Program.cs](c:\Users\Rezio\Desktop\AUBSoftwareIntern\InternshipPractice3\book_Managment\book_Managment\Program.cs):

```csharp
Server=DESKTOP-RHMPA0K\SQLEXPRESS;Database=book_management;Trusted_Connection=True;TrustServerCertificate=True;
```

Important note:

- the app is currently tied to the local machine and SQL Server instance above
- if someone else runs this project, they will likely need to update the connection string first

## Expected Database Objects

Based on the code, the program expects these tables:

- `books_manage`
- `members_manage`
- `borrow_history`

It also expects these stored procedures to already exist:

- `sp_GetAllBooks`
- `sp_InsertBook`
- `sp_UpdateBook`
- `sp_DeleteBook`
- `sp_GetAllMembers`
- `sp_InsertMember`
- `sp_UpdateMember`
- `sp_DeleteMember`
- `sp_GetAllHistory`
- `sp_InsertHistory`
- `sp_UpdateHistory`
- `sp_DeleteHistory`

The code also assumes the tables use an `Id` column and related fields such as:

- `Title`, `Author`, `Publish`, `Category`, `Total`
- `Name`, `Dob`, `Member_Since`
- `BookID`, `MemberID`, `Borrowed_Date`, `Returned_Date`

## Menu Behavior

### Books

The books menu can:

- view all books
- add a new book
- update a book by `Id`
- delete a book by `Id`

Displayed fields:

- `Id`
- `Title`
- `Author`
- `Publish`
- `Category`
- `Total`

### Members

The members menu can:

- view all members
- add a new member
- update a member by `Id`
- delete a member by `Id`

Displayed fields:

- `Id`
- `Name`
- `Dob`
- `Member_Since`

### Borrow History

The history menu can:

- view all borrow records
- add a borrow record
- update a borrow record
- delete a borrow record

Displayed fields:

- `Id`
- `BookID`
- `MemberID`
- `Borrowed_Date`
- `Returned_Date`

If `Returned_Date` is null, the program shows `Not Returned`.

## Validation And Safety Rules

The code includes several helper methods for input validation:

- `BookTitleFormat(...)`: allows any non-empty title
- `NameFormat(...)`: allows only letters and spaces
- `NumberFormat(...)`: allows only non-negative integers
- `DateFormat(...)`: requires `yyyy-MM-dd`
- `OptionalDateFormat(...)`: allows a blank returned date
- `CancelInput(...)`: lets the user type `cancel` to stop an operation

Important behavior:

- books and members cannot be deleted if related rows still exist in `borrow_history`
- history add/update checks that the referenced book and member IDs exist first
- history update also checks that the selected history record exists

## Important Notes About The Current Code

- The whole program is written in one file with nested local functions. This works, but it makes the app harder to maintain as it grows.
- The startup query reads table names from `INFORMATION_SCHEMA.TABLES`, but printing is commented out, so the list is not shown.
- Book titles are validated separately from names, which is useful because titles can contain characters that `NameFormat(...)` would reject.
- `NameFormat(...)` only accepts letters and spaces, which means values with numbers or symbols are rejected. That may be too strict for some real names or categories.
- The program depends heavily on stored procedures and an existing SQL schema. It will not work correctly unless the database is already prepared.

## How To Run

1. Make sure SQL Server is available and the `book_management` database exists.
2. Make sure the required tables and stored procedures are created.
3. Update the connection string in [Program.cs](c:\Users\Rezio\Desktop\AUBSoftwareIntern\InternshipPractice3\book_Managment\book_Managment\Program.cs) if your SQL Server instance is different.
4. Run the project:

```powershell
dotnet build
dotnet run
```

## Current Status

The project builds successfully in its current state.
