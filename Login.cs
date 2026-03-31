using System;
using System.Linq;
using System.Text;

namespace book_Managment
{
    internal class Login
    {
        internal static void Show()
        {
            Login login = new Login();
            login.ShowLogin();
        }

        private string adminUsername = "Sombath";
        private string adminPassword = "admin123";
        private string userUsername = "User";
        private string userPassword = "user123";

        public void ShowLogin()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Welcome to the Book Management System!");
                CancelHint();

                try
                {
                    Console.WriteLine("Hint -> Admin -> UserName: Sombath, Pass: admin123");
                    Console.WriteLine("User: UserName: User, Pass: user123" + '\n');
                    string username = UsernameFormat("Enter your username: ");
                    string password = PasswordFormat("Enter your password: ");

                    if (username == adminUsername && password == adminPassword)
                    {
                        Console.Clear();
                        Console.WriteLine("Login as Admin successful.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        Console.Clear();

                        AdminDashboard.Show();
                        return;
                    }
                    else if (username == userUsername && password == userPassword)
                    {
                        Console.Clear();
                        Console.WriteLine("Login as User successful.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();

                        UserDashboard.Show();
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("Invalid username or password.");
                        Console.WriteLine("Press any key to try again...");
                        Console.ReadKey();
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Login canceled.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }
            }
        }

        static void CancelHint()
        {
            Console.WriteLine("Press Escape key to stop and go back.");
            Console.WriteLine();
        }

        static string CancelInput(string inputTxt)
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

        static string UsernameFormat(string message)
        {
            while (true)
            {
                string input = CancelInput(message).Trim();

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Username cannot be empty.");
                    continue;
                }

                if (input.Length < 3)
                {
                    Console.WriteLine("Username must be at least 3 characters.");
                    continue;
                }

                if (!input.All(c => char.IsLetterOrDigit(c)))
                {
                    Console.WriteLine("Username must contain only letters and numbers.");
                    continue;
                }

                return input;
            }
        }

        static string PasswordFormat(string message)
        {
            while (true)
            {
                string input = PasswordInput(message);

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Password cannot be empty.");
                    continue;
                }

                if (input.Length < 6)
                {
                    Console.WriteLine("Password must be at least 6 characters.");
                    continue;
                }

                return input;
            }
        }
        static string PasswordInput(string message)
        {
            Console.Write(message);
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
                    Console.Write("*");
                }
            }
        }
    }
}
