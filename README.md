The Royal Register - Guest List Management System

The Royal Register is a WinForms App / tool that allows you to manage and maintain a guest list for an event such as a party, wedding, holiday booking, etc. It includes functionality to add, remove, and view guests, along with the ability to export the guest list to a PDF. 
It also allows users to manage guest details like their name, identity number, and contact information.

Features:

1. Add Guest: Allows adding a guest with their full name and identity number.
2. Remove Guest: Allows removing a guest based on their identity number.
3. Contact Number: Each guest has an optional contact number.
4. Export Guest List: Export the guest list to a PDF with all relevant details.
5. Reset Fields: Clear all input fields for new data entry.

Technologies Used:

1. C# with .NET Framework (Windows Forms)
2. SQL Server for database management
3. iTextSharp for PDF generation

Software Requirements:

1. Microsoft Visual Studio (Windows Forms Application)
2. SQL Server (Express or full edition)
3. .NET Framework 4.7.2 (or later)

Database Setup:

1. Create a SQL Server database called RoyalRegister.
2. Create a table Guests in the RoyalRegister database using the following SQL:

CREATE TABLE Guests (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    IdentityNumber VARCHAR(50) NOT NULL,
    FullName VARCHAR(100) NOT NULL,
    ContactNumber VARCHAR(20)
);

Connection String Configuration: Make sure to update the connectionString in Form1.cs with your actual SQL Server credentials.


Functionality Overview:

1. Add Guest
The user can input a guest's name and identity number.
If the identity number already exists, a message will indicate that the guest is already in the register.
The guest's details are saved into the database on table [dbo].[Guests]

2. Remove Guest
The user can input a guest's identity number to remove them from the register.

3. Export Guest List to PDF
The user can export the entire guest list onto a PDF document.
The guest list PDF is created using the iTextSharp library.

4. Reset Fields
Clears all the textboxes and resets the ForeColor of the textboxes to black.

5.To Add a Guest:
Enter the guest's full name and identity number in the respective fields.
Click the "Check / Add Guest" button to add the guest to the database.

6.To Remove a Guest:
Enter the guest's identity number in the "Guest ID Number" field.
Click the "Remove Guest" button to remove the guest from the database.

7.To Export the Guest List:
Click the "Export Guest List" button to export the current guest list into a PDF.

8.To Reset Fields:
Click the "Reset" button to clear the textboxes and reset the colors.
