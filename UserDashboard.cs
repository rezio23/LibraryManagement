using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace book_Managment
{
    internal class UserDashboard
    {
        private const string connectionString = "Server=DESKTOP-RHMPA0K\\SQLEXPRESS;Database=book_management;Trusted_Connection=True;TrustServerCertificate=True;";

        public static void Show()
        {
            Console.Clear();
            while (true)
            {
                Console.WriteLine("1. Book's Data");
                Console.WriteLine("2. Member's Data");
                Console.WriteLine("3. Book's History");
                Console.WriteLine("4. Logout");
                Console.Write("Enter the Main Dashboard's value: ");

                switch (Console.ReadLine())
                {
                    case "1": Console.Clear(); BookMenu(); break;
                    case "2": Console.Clear(); MemberMenu(); break;
                    case "3": Console.Clear(); HistoryMenu(); break;
                    case "4": Console.Clear(); Logout(); return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again." + '\n');
                        break;
                }

            }
        }

        private static void Logout()
        {
            Console.WriteLine("Logging out..." + '\n');
            Pause();
            Login.Show();
        }

        // -------------------- Book --------------------
        private static void BookMenu()
        {
            Console.Clear();
            while (true)
            {
                Console.WriteLine("1. View Books");
                Console.WriteLine("2. Add Books");
                Console.WriteLine("3. Back");
                Console.Write("Enter book's menu: ");

                switch (Console.ReadLine())
                {
                    case "1": ViewBook(); Pause(); break;
                    case "2": AddBook(); break;
                    case "3": Console.Clear(); return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again." + '\n');
                        break;
                }
            }
        }

        private static void ViewBook()
        {
            Console.Clear();

            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            using SqlCommand command = new SqlCommand("sp_GetAllBooks", connection);
            command.CommandType = CommandType.StoredProcedure;

            using SqlDataReader reader = command.ExecuteReader();

            Console.WriteLine($"\n{"ID",-5} {"Title",-30} {"Author",-25} {"Year",-8} {"Category",-15} {"Total",-6}");
            Console.WriteLine(new string('-', 95));

            while (reader.Read())
            {
                Console.WriteLine($"{reader["Id"],-5} {reader["Title"],-30} {reader["Author"],-25} {reader["Publish"],-8} {reader["Category"],-15} {reader["Total"],-6}");
            }

            Console.WriteLine(new string('-', 95));
            Console.WriteLine();
        }

        private static void AddBook()
        {
            Console.Clear();

            try
            {
                ViewBook();
                CancelHint();

                string bookTitle = BookTitleFormat("Enter Book's Title: ");
                string authorName = NameFormat("Enter Author's Name: ");
                int publishYear = YearFormat("Enter Publish's Year: ");
                string bookCategory = NameFormat("Enter Book's Category: ");
                int totalBooks = NumberFormat("Enter Book's Total: ");

                using SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();

                if (DuplicateBookExists(bookTitle, authorName))
                {
                    Console.WriteLine("This book already exists. Please try again." + '\n');
                    return;
                }

                using SqlCommand command = new SqlCommand("sp_InsertBook", connection);
                command.CommandType = CommandType.StoredProcedure;

                AddParams(command,
                    ("@Title", bookTitle),
                    ("@Author", authorName),
                    ("@Publish", publishYear),
                    ("@Category", bookCategory),
                    ("@Total", totalBooks)
                );

                int row = command.ExecuteNonQuery();
                ViewBook();
                Console.WriteLine(row > 0 ? "1 row inserted!\n" : "Insert failed!\n");
                Pause();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation canceled. Returning to book menu." + '\n');
                Pause();
            }
        }

        // -------------------- Member --------------------
        private static void MemberMenu()
        {
            Console.Clear();
            while (true)
            {
                Console.WriteLine("1. View Members");
                Console.WriteLine("2. Add Members");
                Console.WriteLine("3. Back");
                Console.Write("Enter member's menu: ");

                switch (Console.ReadLine())
                {
                    case "1": ViewMember(); Pause(); break;
                    case "2": AddMember(); break;
                    case "3": Console.Clear(); return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again." + '\n');
                        break;
                }
            }
        }

        private static void ViewMember()
        {
            Console.Clear();

            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            using SqlCommand command = new SqlCommand("sp_GetAllMembers", connection);
            command.CommandType = CommandType.StoredProcedure;

            using SqlDataReader reader = command.ExecuteReader();

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

        private static void AddMember()
        {
            Console.Clear();

            try
            {
                ViewMember();
                CancelHint();

                string memberName = NameFormat("Enter Member's Name: ");

                DateTime memberDOB;
                int age;

                while (true)
                {
                    memberDOB = DateFormat("Enter Member's DOB (yyyy-MM-dd): ");

                    age = DateTime.Today.Year - memberDOB.Year;
                    if (memberDOB.Date > DateTime.Today.AddYears(-age))
                        age--;

                    if (age < 15)
                    {
                        Console.WriteLine("Member's age should be at least 15 years old!" + '\n'); continue;
                    }

                    break;
                }

                DateTime memberShip;
                while (true)
                {
                    memberShip = DateFormat("Enter Member's Membership Year (yyyy-MM-dd): ");

                    if (memberShip.Date > DateTime.Today)
                    {
                        Console.WriteLine("Membership year cannot be in the future!" + '\n'); continue;
                    }

                    if (memberShip.Date <= memberDOB.Date)
                    {
                        Console.WriteLine("Membership date must be after DOB!" + '\n'); continue;
                    }

                    break;
                }

                using SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();

                using SqlCommand command = new SqlCommand("sp_InsertMember", connection);
                command.CommandType = CommandType.StoredProcedure;

                AddParams(command,
                    ("@Name", memberName),
                    ("@Dob", memberDOB),
                    ("@Member_Since", memberShip)
                );

                int row = command.ExecuteNonQuery();

                ViewMember();
                Console.WriteLine(row > 0 ? "1 row inserted!\n" : "Insert failed!\n");
                Pause();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation canceled. Returning to member menu." + '\n');
                Pause();
            }
        }

        // -------------------- History --------------------
        private static void HistoryMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("1. View History");
                Console.WriteLine("2. Add History");
                Console.WriteLine("3. Back");
                Console.Write("Enter history's menu: ");

                switch (Console.ReadLine())
                {
                    case "1": ViewHistory(); Pause(); break;
                    case "2": AddHistory(); break;
                    case "3": Console.Clear(); return;
                    default: Console.WriteLine("Invalid choice. Please try again." + '\n'); break;
                }
            }
        }

        private static void ViewHistory()
        {
            Console.Clear();

            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            using SqlCommand command = new SqlCommand("sp_GetAllHistory", connection);
            command.CommandType = CommandType.StoredProcedure;

            using SqlDataReader reader = command.ExecuteReader();

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
        }

        private static void AddHistory()
        {
            Console.Clear();
            try
            {
                ViewHistory();
                CancelHint();

                using SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();

                int bookId;
                while (true)
                {
                    bookId = NumberFormat("Enter Book's ID: ");
                    if (ExistedId("books_manage", bookId))
                        break;

                    Console.WriteLine("Book ID not found. Please try again." + '\n');
                }

                int memberId;
                while (true)
                {
                    memberId = NumberFormat("Enter Member's ID: ");
                    if (ExistedId("members_manage", memberId))
                        break;

                    Console.WriteLine("Member ID not found. Please try again." + '\n');
                }

                DateTime borrowedDate;
                while (true)
                {
                    borrowedDate = DateFormat("Enter Borrowed Date (yyyy-MM-dd): ");
                    if (borrowedDate.Date <= DateTime.Today)
                        break;

                    Console.WriteLine("Borrowed date cannot be in the future. Please try again." + '\n');
                }

                object returnedValue;
                while (true)
                {
                    returnedValue = OptionalDateFormat("Enter Returned Date (yyyy-MM-dd), or press Enter to skip: ");

                    if (returnedValue == DBNull.Value)
                        break;

                    DateTime returnedDate = (DateTime)returnedValue;
                    if (returnedDate.Date > borrowedDate.Date)
                        break;

                    Console.WriteLine("Returned date must be after borrowed date. Please try again." + '\n');
                }

                if (ActiveBorrowExists(bookId, memberId))
                {
                    Console.WriteLine("This member is already borrowing this book and hasn't returned it yet!" + '\n');
                    Pause();
                    return;
                }

                using SqlCommand command = new SqlCommand("sp_InsertHistory", connection);
                command.CommandType = CommandType.StoredProcedure;

                AddParams(command,
                     ("@BookID", bookId),
                     ("@MemberID", memberId),
                     ("@Borrowed_Date", borrowedDate),
                     ("@Returned_Date", returnedValue)
                );

                int row = command.ExecuteNonQuery();

                ViewHistory();
                Console.WriteLine(row > 0 ? "1 row inserted!\n" : "Insert failed!\n");
                Pause();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation canceled. Returning to history menu." + '\n');
                Pause();
            }
        }

        // -------------------- Validation --------------------
        private static void Pause()
        {
            Console.WriteLine("Press any key to continue..." + '\n');
            Console.ReadKey(true);
        }

        private static void CancelHint()
        {
            Console.WriteLine("Press Escape key to stop and go back." + '\n');
            Console.WriteLine();
        }

        private static string CancelInput(string inputTxt)
        {
            Console.Write(inputTxt);

            StringBuilder input = new StringBuilder();

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine();
                    throw new OperationCanceledException();
                }

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return input.ToString();
                }

                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0)
                    {
                        input.Remove(input.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                    continue;
                }

                if (!char.IsControl(keyInfo.KeyChar))
                {
                    input.Append(keyInfo.KeyChar);
                    Console.Write(keyInfo.KeyChar);
                }
            }
        }

        private static string BookTitleFormat(string title)
        {
            while (true)
            {
                string input = CancelInput(title);

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Input cannot be empty. Please try again." + '\n');
                    Console.WriteLine();
                    continue;
                }

                return input;
            }
        }

        private static string NameFormat(string name)
        {
            while (true)
            {
                string input = CancelInput(name);

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Input cannot be empty. Please try again." + '\n');
                    Console.WriteLine();
                    continue;
                }

                if (!input.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                {
                    Console.WriteLine("Input must contain only letters and spaces. Please try again." + '\n');
                    Console.WriteLine();
                    continue;
                }

                return input;
            }
        }

        private static int NumberFormat(string number)
        {
            while (true)
            {
                string input = CancelInput(number);

                if (!int.TryParse(input, out int result))
                {
                    Console.WriteLine("Invalid! Please enter a number." + '\n');
                    continue;
                }

                if (result < 0)
                {
                    Console.WriteLine("Number cannot be negative. Please try again." + '\n');
                    continue;
                }

                return result;
            }
        }

        private static int YearFormat(string number)
        {
            while (true)
            {
                string input = CancelInput(number);

                if (!int.TryParse(input, out int result))
                {
                    Console.WriteLine("Invalid! Please enter a number." + '\n');
                    continue;
                }

                if (result < 0)
                {
                    Console.WriteLine("Number cannot be negative. Please try again." + '\n');
                    Console.WriteLine();
                    continue;
                }

                if (result > DateTime.Today.Year)
                {
                    Console.WriteLine("Year cannot be in the future. Please try again." + '\n');
                    Console.WriteLine();
                    continue;
                }

                if (result < 1000)
                {
                    Console.WriteLine("Year must be a 4-digit number. Please try again." + '\n');
                    Console.WriteLine();
                    continue;
                }

                return result;
            }
        }

        private static DateTime DateFormat(string date)
        {
            while (true)
            {
                string input = CancelInput(date);

                if (DateTime.TryParseExact(
                    input,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime result))
                {
                    return result;
                }

                Console.WriteLine("Invalid! Use format yyyy-MM-dd." + '\n');
            }
        }

        private static object OptionalDateFormat(string optionalDate)
        {
            while (true)
            {
                string input = CancelInput(optionalDate);

                if (string.IsNullOrWhiteSpace(input))
                    return DBNull.Value;

                if (DateTime.TryParseExact(
                    input,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime result))
                {
                    return result;
                }

                Console.WriteLine("Invalid! Use format yyyy-MM-dd or press Enter to skip." + '\n');
            }
        }

        private static bool ExistedId(string tableName, int id)
        {
            return tableName switch
            {
                "books_manage" => BookExists(id),
                "members_manage" => MemberExists(id),
                "borrow_history" => HistoryExists(id),
                _ => throw new ArgumentException("Invalid table name.")
            };
        }
        private static int ExecuteCountProcedure(string procedureName, Action<SqlCommand>? addParameters = null)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            using SqlCommand command = new SqlCommand(procedureName, connection);
            command.CommandType = CommandType.StoredProcedure;

            addParameters?.Invoke(command);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        private static bool BookExists(int id)
        {
            return ExecuteCountProcedure("sp_CheckBookExists", cmd =>
            {
                cmd.Parameters.AddWithValue("@Id", id);
            }) > 0;
        }

        private static bool MemberExists(int id)
        {
            return ExecuteCountProcedure("sp_CheckMemberExists", cmd =>
            {
                cmd.Parameters.AddWithValue("@Id", id);
            }) > 0;
        }

        private static bool HistoryExists(int id)
        {
            return ExecuteCountProcedure("sp_CheckHistoryExists", cmd =>
            {
                cmd.Parameters.AddWithValue("@Id", id);
            }) > 0;
        }

        private static bool DuplicateBookExists(string title, string author)
        {
            return ExecuteCountProcedure("sp_CheckDuplicateBook", cmd =>
            {
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Author", author);
            }) > 0;
        }

        private static bool ActiveBorrowExists(int bookId, int memberId)
        {
            return ExecuteCountProcedure("sp_CheckActiveBorrow", cmd =>
            {
                cmd.Parameters.AddWithValue("@BookID", bookId);
                cmd.Parameters.AddWithValue("@MemberID", memberId);
            }) > 0;
        }
        private static void AddParams(SqlCommand command, params (string Name, object? Value)[] parameters)
        {
            foreach (var (name, value) in parameters)
            {
                command.Parameters.AddWithValue(name, value ?? DBNull.Value);
            }
        }
    }
}
