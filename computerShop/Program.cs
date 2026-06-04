using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace computerShop
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1. Create the login form
            using (var loginForm = new LoginForm())
            {
                // 2. Show it as a dialog
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    // 3. If login successful, launch your main form
                    // You can pass the authenticated employee to Form1's constructor
                    Application.Run(new Form1(loginForm.AuthenticatedEmployee));
                }
                else
                {
                    // 4. If login was closed/cancelled, exit the app
                    Application.Exit();
                }
            }
        }
    }
}
