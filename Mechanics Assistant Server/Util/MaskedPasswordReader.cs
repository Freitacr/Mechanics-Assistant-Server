using System;
using System.Threading;
using System.Collections.Generic;
using System.Security;

namespace OldManInTheShopServer.Util
{
    class MaskedPasswordReader
    {
        /// <summary>
        /// Retrieves a password that, unfortunately, is stored in plaintext from the user while keeping their console masked.
        /// </summary>
        /// <param name="prompt">Prompt to display to the user before they are asked to input values</param>
        /// <returns>A string containing the password in plaintext because MySQL requires a plaintext password. For Reasons. Ugh.</returns>
        public static SecureString ReadPasswordMasked(string prompt)
        {
            Console.WriteLine(prompt);
            SecureString ret = new SecureString();

            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                    {
                        if (ret.Length > 0)
                        {
                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                            for (int i = 0; i < ret.Length; i++)
                                Console.Write(' ');
                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                            ret.Clear();
                            continue;
                        }
                    }
                    else
                    {
                        if (ret.Length > 0)
                        {
                            ret.RemoveAt(ret.Length - 1);
                            Console.Write("\b \b"); //So this is weird. The escaped b means to pull the writing cursor back one space. So the cursor will move, then write a space, then move behind the space
                            continue;
                        }
                    }
                }
                else
                {
                    if (key.Key == ConsoleKey.Delete || (key.Key == ConsoleKey.Z && key.Modifiers.HasFlag(ConsoleModifiers.Control)))
                        throw new ThreadInterruptedException();
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        Console.Write('\n');
                        continue;
                    }
                    else
                    {
                        ret.AppendChar(key.KeyChar);
                        Console.Write('*');
                    }
                }
            } while (key.Key != ConsoleKey.Enter);

            return ret;
        } 
    }
}
