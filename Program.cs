using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Net;

// Database Connection
string connectionString = "Server=DESKTOP-RHMPA0K\\SQLEXPRESS;Database=book_management;Trusted_Connection=True;TrustServerCertificate=True;";
using (SqlConnection connection = new SqlConnection(connectionString))
{
    connection.Open();
    Console.WriteLine("Connected to book_management!");

    string query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";

    using (SqlCommand command = new SqlCommand(query, connection))
    using (SqlDataReader reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            //Console.WriteLine("Select by : " + reader["TABLE_NAME"]);
        }
    }
}

bool run = true;

while (run)
{
    Default();

    void Default()
    {
        Console.WriteLine("1. Book's Data");
        Console.WriteLine("2. Member's Data");
        Console.WriteLine("3. Book's History");
        Console.WriteLine("4. Exit");
        Console.Write("Enter the Main Dashboard's value: ");

        switch (Console.ReadLine())
        {
            case "1": Book(); break;
            case "2": Member(); break;
            case "3": History(); break;
            case "4": run = false; break;
            default: Console.WriteLine("Invalid choice. Please try again."); break;
        }

        // Book's Data -------------------------------------------------------------------

        void Book()
        {
            Console.WriteLine();
            Console.WriteLine("1.1 View Books");
            Console.WriteLine("1.2 Add Books");
            Console.WriteLine("1.3 Update Books");
            Console.WriteLine("1.4 Delete Books");
            Console.WriteLine("1.5 Back");
            Console.Write("Enter book's menu: ");

            switch (Console.ReadLine())
            {
                case "1.1": ViewBook(); break;
                case "1.2": AddBook(); break;
                case "1.3": UpdateBook(); break;
                case "1.4": DeleteBook(); break;
                case "1.5": Default(); break;
                default: Console.WriteLine("Invalid choice. Please try again."); break;
            }

            void ViewBook()
            {
                Console.WriteLine();
                SqlConnection? connection = null;
                SqlCommand? command = null;
                SqlDataReader? reader = null;

                connection = new SqlConnection(connectionString);
                connection.Open();

                //string query = "SELECT * FROM books_manage";
                //using SqlCommand command = new SqlCommand(query, connection);
                command = new SqlCommand("sp_GetAllBooks", connection);
                command.CommandType = CommandType.StoredProcedure; // Procedure

                reader = command.ExecuteReader();
                {
                    Console.WriteLine($"\n{"ID",-5} {"Title",-30} {"Author",-25} {"Year",-8} {"Category",-15} {"Total",-6}");
                    Console.WriteLine(new string('-', 95));
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["Id"],-5} {reader["Title"],-30} {reader["Author"],-25} {reader["Publish"],-8} {reader["Category"],-15} {reader["Total"],-6}");
                    }

                    Console.WriteLine(new string('-', 95));
                    Console.WriteLine();
                }
            }
            void AddBook()
            {
                try
                {
                    ViewBook();
                    CancelHint();

                    SqlCommand? command = null;
                    SqlConnection? connection = null;

                    string bookTitle = BookTitleFormat("Enter Book's Title: ");

                    string authorName = NameFormat("Enter Author's Name: ");

                    int publishYear = NumberFormat("Enter Publish's Year: ");

                    string bookCategory = NameFormat("Enter Book's Category: ");

                    int totalBooks = NumberFormat("Enter Book's Total: ");

                    connection = new SqlConnection(connectionString);
                    connection.Open();

                    command = new SqlCommand("sp_InsertBook", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Title", bookTitle);
                    command.Parameters.AddWithValue("@Author", authorName);
                    command.Parameters.AddWithValue("@Publish", publishYear);
                    command.Parameters.AddWithValue("@Category", bookCategory);
                    command.Parameters.AddWithValue("@Total", totalBooks);

                    int row = command.ExecuteNonQuery();
                    Console.WriteLine("Inserted 1 data!");

                    Book();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to book menu.");
                    ViewBook();
                    Book();
                }
            }
            void UpdateBook()
            {
                try
                {
                    ViewBook();
                    CancelHint();

                    int updateBookId = NumberFormat("Select ID to update: ");

                    string newBookTitle = BookTitleFormat("Enter new Book's Title: ");
                    string authorName = NameFormat("Enter new Author's Name: ");
                    int publishYear = NumberFormat("Enter new Publish's Year: ");
                    string bookCategory = NameFormat("Enter new Book's Category: ");
                    int totalBooks = NumberFormat("Enter new Book's Total: ");

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using SqlCommand command = new SqlCommand("sp_UpdateBook", connection);
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Id", updateBookId);
                        command.Parameters.AddWithValue("@Title", newBookTitle);
                        command.Parameters.AddWithValue("@Author", authorName);
                        command.Parameters.AddWithValue("@Publish", publishYear);
                        command.Parameters.AddWithValue("@Category", bookCategory);
                        command.Parameters.AddWithValue("@Total", totalBooks);

                        int row = command.ExecuteNonQuery();
                        Console.WriteLine(row > 0 ? "Updated!" : "ID not found!");

                        Book();
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to book menu.");
                    ViewBook();
                    Book();
                }
            }
            void DeleteBook()
            {
                try
                {
                    ViewBook();
                    CancelHint();

                    int deleteId = NumberFormat("Select ID to delete: ");

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string checkQuery = "SELECT COUNT(*) FROM borrow_history WHERE BookID = @Id"; // Check if the book is existed
                        using SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                        checkCommand.Parameters.AddWithValue("@Id", deleteId);
                        int refCount = (int)checkCommand.ExecuteScalar();

                        if (refCount > 0)
                        {
                            Console.WriteLine($"Cannot delete! This book has {refCount} borrow record(s) in history.");
                            Console.WriteLine("Delete the borrow history first, then try again.");
                            return;
                        }

                        using SqlCommand command = new SqlCommand("sp_DeleteBook", connection);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Id", deleteId);

                        int row = command.ExecuteNonQuery();
                        Console.WriteLine(row > 0 ? "1 row deleted!" : "ID not found! Please try again!");
                        Console.WriteLine();

                        Book();
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to book menu.");
                    ViewBook();
                    Book();
                }
            }
        }
        // Member's Data -------------------------------------------------------------------
        void Member()
        {
            Console.WriteLine("2.1 View Members");
            Console.WriteLine("2.2 Add Members");
            Console.WriteLine("2.3 Update Members");
            Console.WriteLine("2.4 Delete Members");
            Console.WriteLine("2.5 Back");
            Console.Write("Enter member's menu: ");

            switch (Console.ReadLine())
            {
                case "2.1": ViewMember(); break;
                case "2.2": AddMember(); break;
                case "2.3": UpdateMember(); break;
                case "2.4": DeleteMember(); break;
                case "2.5": Default(); break;
                default: Console.WriteLine("Invalid choice. Please try again."); break;
            }

            void ViewMember()
            {
                Console.WriteLine();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using SqlCommand command = new SqlCommand("sp_GetAllMembers", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    using SqlDataReader reader = command.ExecuteReader();
                    {
                        Console.WriteLine($"\n{"ID",-5} {"Name",-20} {"Date of Birth",-15} {"Member Since",-15}");
                        Console.WriteLine(new string('-', 60));
                        while (reader.Read())
                        {
                            DateTime dob = Convert.ToDateTime(reader["Dob"]);
                            DateTime since = Convert.ToDateTime(reader["Member_Since"]);

                            Console.WriteLine($"{reader["Id"],-5} {reader["Name"],-20} {dob.ToShortDateString(),-15} {since.ToShortDateString(),-15}");
                        }

                        Console.WriteLine(new string('-', 60));
                        Console.WriteLine();
                    }
                }
            }
            void AddMember()
            {
                try
                {
                    ViewMember();
                    CancelHint();

                    SqlConnection? connection = null;
                    SqlCommand? command = null;

                    string memberName = NameFormat("Enter Member's Name: ");
                    DateTime memberDOB = DateFormat("Enter Member's DOB (yyyy-MM-dd): ");
                    DateTime memberShip = DateFormat("Enter Member's Membership Year (yyyy-MM-dd): ");

                    connection = new SqlConnection(connectionString);
                    connection.Open();

                    command = new SqlCommand("sp_InsertMember", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Name", memberName);
                    command.Parameters.AddWithValue("@Dob", memberDOB);
                    command.Parameters.AddWithValue("@Member_Since", memberShip);

                    int row = command.ExecuteNonQuery();
                    Console.WriteLine("1 row inserted!");

                    ViewMember();
                    Member();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to member menu.");
                    ViewMember();
                    Member();
                }
            }
            void UpdateMember()
            {
                try
                {
                    ViewMember();
                    CancelHint();

                    int updateMemberId = NumberFormat("Select ID to Update: ");
                    string updateMemberName = NameFormat("Enter new Member's Name: ");
                    DateTime updateMemberDOB = DateFormat("Enter new Member's DOB (yyyy-MM-dd): ");
                    DateTime updateMemberShip = DateFormat("Enter new Member's Membership Year (yyyy-MM-dd): ");

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        using SqlCommand command = new SqlCommand("sp_UpdateMember", connection);
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Id", updateMemberId);
                        command.Parameters.AddWithValue("@Name", updateMemberName);
                        command.Parameters.AddWithValue("@Dob", updateMemberDOB);
                        command.Parameters.AddWithValue("@Member_Since", updateMemberShip);
                        int row = command.ExecuteNonQuery();
                        Console.WriteLine(row > 0 ? "Updated!" : "ID not found! Please try again!");

                        ViewMember();
                        Member();
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to member menu.");
                    ViewMember();
                    Member();
                }

            }
            void DeleteMember()
            {
                try
                {
                    ViewMember();
                    CancelHint();

                    SqlCommand? command = null;
                    SqlConnection? connection = null;

                    int deleteMemberId = NumberFormat("Select ID to Delete: ");

                    connection = new SqlConnection(connectionString);
                    connection.Open();

                    string checkQuery = "SELECT COUNT(*) FROM borrow_history WHERE MemberID = @Id";
                    using SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@Id", deleteMemberId);
                    int refCount = (int)checkCommand.ExecuteScalar();

                    if (refCount > 0)
                    {
                        Console.WriteLine($"Cannot delete! This member has {refCount} borrow record(s) in history.");
                        Console.WriteLine("Delete the borrow history first, then try again.");
                        return;
                    }

                    command = new SqlCommand("sp_DeleteMember", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Id", deleteMemberId);
                    int row = command.ExecuteNonQuery();
                    Console.WriteLine(row > 0 ? "1 row deleted!" : "ID not found! Please try again!");

                    ViewMember();
                    Member();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to member menu.");
                    ViewMember();
                    Member();
                }
            }
        }

        // History's Data -------------------------------------------------------------------
        void History()
        {
            Console.WriteLine("3.1 View History");
            Console.WriteLine("3.2 Add History");
            Console.WriteLine("3.3 Update History");
            Console.WriteLine("3.4 Delete History");
            Console.WriteLine("3.5 Back");
            Console.Write("Enter history's menu: ");

            switch (Console.ReadLine())
            {
                case "3.1": ViewHistory(); break;
                case "3.2": AddHistory(); break;
                case "3.3": UpdateHistory(); break;
                case "3.4": DeleteHistory(); break;
                case "3.5": Default(); break;
                default: Console.WriteLine("Invalid choice. Please try again."); break;
            }

            void ViewHistory()
            {
                Console.WriteLine();
                SqlConnection? connection = null;
                SqlCommand? command = null;
                SqlDataReader? reader = null;

                connection = new SqlConnection(connectionString);
                connection.Open();

                command = new SqlCommand("sp_GetAllHistory", connection);
                command.CommandType = CommandType.StoredProcedure;

                reader = command.ExecuteReader();
                {
                    Console.WriteLine($"\n{"ID",-5} {"Book ID",-10} {"Member ID",-12} {"Borrowed Date",-15} {"Returned Date",-15}");
                    Console.WriteLine(new string('-', 62));

                    while (reader.Read())
                    {
                        DateTime borrowed = Convert.ToDateTime(reader["Borrowed_Date"]);
                        string returned = reader["Returned_Date"] == DBNull.Value
                            ? "Not Returned"
                            : Convert.ToDateTime(reader["Returned_Date"]).ToShortDateString();

                        Console.WriteLine($"{reader["Id"],-5} {reader["BookID"],-10} {reader["MemberID"],-12} {borrowed.ToShortDateString(),-15} {returned,-15}");
                    }

                    Console.WriteLine(new string('-', 62));
                    Console.WriteLine();
                }
            }
            void AddHistory()
            {
                try
                {
                    ViewHistory();
                    CancelHint();

                    SqlConnection? connection = null;
                    SqlCommand? command = null;

                    int bookId = NumberFormat("Enter Book's ID: ");
                    int memberId = NumberFormat("Enter Member's ID: ");
                    DateTime borrowedDate = DateFormat("Enter Borrowed Date (yyyy-MM-dd): ");

                    object returnedValue = OptionalDateFormat("Enter Returned Date (yyyy-MM-dd), or press Enter to skip: ");
                    //string returnedDate = OptionalDateFormat("Enter Returned Date (yyyy-MM-dd), or press Enter to skip: ").ToString();
                    //object returnedValue = string.IsNullOrWhiteSpace(returnedDate)
                    //    ? (object)DBNull.Value
                    //    : DateTime.Parse(returnedDate);

                    connection = new SqlConnection(connectionString);
                    connection.Open();
                    if (!ExistedId(connection, "books_manage", bookId))
                    {
                        Console.WriteLine("Book ID not found. Please try again.");
                        return;
                    }

                    if (!ExistedId(connection, "members_manage", memberId))
                    {
                        Console.WriteLine("Member ID not found. Please try again.");
                        return;
                    }

                    command = new SqlCommand("sp_InsertHistory", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@BookID", bookId);
                    command.Parameters.AddWithValue("@MemberID", memberId);
                    command.Parameters.AddWithValue("@Borrowed_Date", borrowedDate);
                    command.Parameters.AddWithValue("@Returned_Date", returnedValue);
                    int row = command.ExecuteNonQuery();
                    Console.WriteLine("1 row inserted!");

                    ViewHistory();
                    History();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to history menu.");
                    ViewHistory();
                    History();
                }
            }
            void UpdateHistory()
            {
                try
                {
                    ViewHistory();
                    CancelHint();

                    SqlConnection? connection = null;
                    SqlCommand? command = null;

                    int updateHistoryId = NumberFormat("Select ID to Update: ");
                    int newBookId = NumberFormat("Enter new Book's ID: ");
                    int newMemberId = NumberFormat("Enter new Member's ID: ");
                    DateTime newBorrowedDate = DateFormat("Enter new Borrowed Date (yyyy-MM-dd): ");
                    object returnedValue = OptionalDateFormat("Enter new Returned Date (yyyy-MM-dd), or press Enter to skip: ");

                    connection = new SqlConnection(connectionString);
                    connection.Open();
                    if (!ExistedId(connection, "books_manage", newBookId))
                    {
                        Console.WriteLine("Book ID not found. Please try again.");
                        return;
                    }

                    if (!ExistedId(connection, "members_manage", newMemberId))
                    {
                        Console.WriteLine("Member ID not found. Please try again.");
                        return;
                    }

                    if (!ExistedId(connection, "borrow_history", updateHistoryId))
                    {
                        Console.WriteLine("History ID not found. Please try again.");
                        return;
                    }
                    command = new SqlCommand("sp_UpdateHistory", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Id", updateHistoryId);
                    command.Parameters.AddWithValue("@BookID", newBookId);
                    command.Parameters.AddWithValue("@MemberID", newMemberId);
                    command.Parameters.AddWithValue("@Borrowed_Date", newBorrowedDate);
                    command.Parameters.AddWithValue("@Returned_Date", returnedValue);

                    int row = command.ExecuteNonQuery();
                    Console.WriteLine(row > 0 ? "Updated!" : "ID not found!");

                    ViewHistory();
                    History();

                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to history menu.");
                    ViewHistory();
                    History();
                }
            }
            void DeleteHistory()
            {
                try
                {

                    ViewHistory();
                    CancelHint();

                    SqlConnection? connection = null;
                    SqlCommand? command = null;

                    int deleteHistoryId = NumberFormat("Select ID to Delete: ");

                    connection = new SqlConnection(connectionString);
                    connection.Open();
                    command = new SqlCommand("sp_DeleteHistory", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Id", deleteHistoryId);
                    int row = command.ExecuteNonQuery();
                    Console.WriteLine(row > 0 ? "1 row deleted!" : "ID not found!");
                    ViewHistory();
                    History();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to history menu.");
                    ViewHistory();
                    History();
                }
            }
        }
        static void CancelHint()
        {
            Console.WriteLine("Type 'cancel' anytime to stop and go back.");
            Console.WriteLine();
        }

        static string CancelInput(string inputTxt)
        {
            Console.Write(inputTxt);
            string? input = Console.ReadLine();

            if (input != null && input.Trim().Equals("cancel", StringComparison.OrdinalIgnoreCase))
                throw new OperationCanceledException();

            return input ?? string.Empty;
        }

        static string BookTitleFormat(string title)
        {
            while (true)
            {
                string input = CancelInput(title);
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Input cannot be empty. Please try again.");
                    continue;
                }
                return input;
            }
        }

        static string NameFormat(string name)
        {
            while (true)
            {
                string input = CancelInput(name);

                // Catch if it was empty
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Input cannot be empty. Please try again.");
                    continue;
                }

                // Catch if it contains non-letter characters
                if (!input.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                {
                    Console.WriteLine("Input must contain only letters and spaces. Please try again.");
                    continue;
                }

                return input;
            }
        }
        static int NumberFormat(string number)
        {
            while (true)
            {
                string input = CancelInput(number);

                if (!int.TryParse(input, out int result))
                {
                    Console.WriteLine("Invalid! Please enter a number.");
                    continue;
                }

                if (result < 0)
                {
                    Console.WriteLine("Number cannot be negative. Please try again.");
                    continue;
                }

                return result;
            }
        }

        static DateTime DateFormat(string date)
        {
            while (true)
            {
                string input = CancelInput(date);
                if (DateTime.TryParseExact(input, "yyyy-MM-dd",
                    null, System.Globalization.DateTimeStyles.None, out DateTime result))
                    return result;
                Console.WriteLine("Invalid! Use format yyyy-MM-dd.");
            }
        }

        static object OptionalDateFormat(string optionalDate)
        {
            while (true)
            {
                string input = CancelInput(optionalDate);

                if (string.IsNullOrWhiteSpace(input))
                    return DBNull.Value;

                if (DateTime.TryParseExact(input, "yyyy-MM-dd",
                    null, System.Globalization.DateTimeStyles.None, out DateTime result))
                    return result;

                Console.WriteLine("Invalid! Use format yyyy-MM-dd or press Enter to skip.");
            }
        }

        static bool ExistedId(SqlConnection connection, string tableName, int id)
        {
            string query = $"SELECT COUNT(*) FROM {tableName} WHERE Id = @Id";

            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            return (int)command.ExecuteScalar() > 0;
        }
    }
}